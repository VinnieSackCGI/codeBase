namespace Plugins_CommonLibrary.Interfaces
{
	public interface IFileData
	{
		string FileName { get; set; }
		string MimeType { get; set; }
		byte[] FileBytes { get; set; }
	}

}
