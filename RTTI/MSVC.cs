using RTTIScanner.ClassExtensions;
using RTTIScanner.Ifaces;
using RTTIScanner.Memory;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading.Tasks;

namespace RTTIScanner.RTTI
{
	public class MSVC : Parser
	{
		// Reference: https://github.com/ReClassNET/ReClass.NET/blob/a02fcb9bd669c8f81facd3ee9ad57cdcbf2cc0e1/ReClass.NET/Memory/RemoteProcess.cs#L190
		public override async Task<string[]> ReadRemoteRuntimeTypeInformation64(IntPtr address)
		{
			if (!address.IsValid())
			{
				return null;
			}

			try
			{
				int baseOffset = await ReadRemoteInt32(address + 0x14);
				if (baseOffset != 0)
				{
					var baseAddress = address - baseOffset;

					var classHierarchyDescriptorOffset = await ReadRemoteInt32(address + 0x10);
					if (classHierarchyDescriptorOffset != 0)
					{
						var classHierarchyDescriptorPtr = baseAddress + classHierarchyDescriptorOffset;

						var baseClassCount = await ReadRemoteInt32(classHierarchyDescriptorPtr + 0x08);
						if (baseClassCount > 0 && baseClassCount < 25)
						{
							var baseClassArrayOffset = await ReadRemoteInt32(classHierarchyDescriptorPtr + 0x0C);
							if (baseClassArrayOffset != 0)
							{
								var baseClassArrayPtr = baseAddress + baseClassArrayOffset;

								var sb = new StringBuilder();
								for (var i = 0; i < baseClassCount; ++i)
								{
									var baseClassDescriptorOffset = await ReadRemoteInt32(baseClassArrayPtr + (4 * i));
									if (baseClassDescriptorOffset != 0)
									{
										var baseClassDescriptorPtr = baseAddress + baseClassDescriptorOffset;

										var typeDescriptorOffset = await ReadRemoteInt32(baseClassDescriptorPtr);
										if (typeDescriptorOffset != 0)
										{
											var typeDescriptorPtr = baseAddress + typeDescriptorOffset;

											var name = await ReadRemoteStringUntilFirstNullCharacter(typeDescriptorPtr + 0x14, Encoding.UTF8, 60);
											if (string.IsNullOrEmpty(name))
											{
												break;
											}

											if (name.EndsWith("@@"))
											{
												name = UndecorateSymbolName("?" + name);
											}

											sb.Append(name);
											sb.Append(" - ");

											continue;
										}
									}

									break;
								}

								if (sb.Length != 0)
								{
									sb.Length -= 3;

									return sb.ToString().Split(separator: new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Catched error reading process memory: {ex.Message}");
			}

			return null;
		}

		public async Task<string> ReadRemoteStringUntilFirstNullCharacter(IntPtr address, Encoding encoding, int length)
		{
			Contract.Requires(encoding != null);
			Contract.Requires(length >= 0);
			Contract.Ensures(Contract.Result<string>() != null);

			var data = await Memory.Reader.GetInstance().GetBytes(address, length * encoding.GuessByteCountPerChar());

			// TODO We should cache the pattern per encoding.
			var index = PatternScanner.FindPattern(BytePattern.From(new byte[encoding.GuessByteCountPerChar()]), data);
			if (index == -1)
			{
				index = data.Length;
			}

			try
			{
				return encoding.GetString(data, 0, Math.Min(index, data.Length));
			}
			catch
			{
				return string.Empty;
			}
		}

		public string UndecorateSymbolName(string name)
		{
			var sb = new StringBuilder(255);
			if (WinAPI.UnDecorateSymbolName(name, sb, sb.Capacity, /*UNDNAME_NAME_ONLY*/0x1000) != 0)
			{
				return sb.ToString();
			}
			return name;
		}
	}
}
