using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;

namespace Plugins_CommonLibrary.Services
{
    using Interfaces;
    /// </summary>
    public class FileData : IFileData
    {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public byte[] FileBytes { get; set; }
    }

    public static class FileContents
    {
        public static string GetMimeType(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            //var extension = System.Web.MimeMapping.GetMimeMapping(filename);
            return filename.Substring(filename.LastIndexOf('.') + 1).ToUpper();
        }

        public static string FileExtension(string fileName)
        {
            return fileName.Substring(fileName.LastIndexOf('.') + 1).ToUpper();
        }

        public static bool LoadFileContents(IOrganizationService orgService, ITracingService tracer,
                     IFileData fileData, string fileAttributeName, string entityLogicalName, Guid recordId)
        {
            var initUpFile = new InitializeFileBlocksUploadRequest()
            {
                FileAttributeName = fileAttributeName,
                FileName = fileData.FileName,
                Target = new EntityReference(entityLogicalName, recordId)
            };

            var initUpFileResponse = (InitializeFileBlocksUploadResponse)orgService.Execute(initUpFile);

            var blockSize = (4 * 1024 * 1024);
            var upfileContinuationToken = initUpFileResponse.FileContinuationToken;
            var blockIds = new List<string>();

            var fileDataBytes = fileData.FileBytes;

            for (var blk = 0; blk < Math.Ceiling((double)fileDataBytes.Length / blockSize); blk++)
            {
                var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
                blockIds.Add(blockId);

                var blockData = fileDataBytes.Skip(blk * blockSize).Take(blockSize).ToArray();
                var blockRequest = new UploadBlockRequest()
                {
                    FileContinuationToken = upfileContinuationToken,
                    BlockId = blockId,
                    BlockData = blockData
                };
                var blockResponse = (UploadBlockResponse)orgService.Execute(blockRequest);
            }

            tracer.Trace("File data loaded.");

            var commitRequest = new CommitFileBlocksUploadRequest()
            {
                BlockList = blockIds.ToArray(),
                FileContinuationToken = upfileContinuationToken,
                FileName = initUpFile.FileName,
                MimeType = GetMimeType(initUpFile.FileName)
            };

            orgService.Execute(commitRequest);

            tracer.Trace("File data committed.");

            return true;
        }

        public static IFileData GetFileContents(IOrganizationService orgService,
                        string fileAttributeName, string entityLogicalName, Guid id)
        {
            var initFileRequest = new InitializeFileBlocksDownloadRequest()
            {
                FileAttributeName = fileAttributeName,
                Target = new EntityReference(entityLogicalName, id)
            };

            InitializeFileBlocksDownloadResponse initFileResponse = null;
            try
            {
                initFileResponse = (InitializeFileBlocksDownloadResponse)orgService.Execute(initFileRequest);
            }
            catch
            {
                return null;
            }

            var fileSize = initFileResponse.FileSizeInBytes;
            var blockSize = (4 * 1024 * 1024);
            var blockOffset = 0;
            byte[] fileBytes = new byte[fileSize];
            var fileContinuationToken = initFileResponse.FileContinuationToken;

            while (blockOffset < fileSize)
            {
                DownloadBlockRequest downloadBlockRequest =
                                            new DownloadBlockRequest()
                                            {
                                                Offset = blockOffset,
                                                BlockLength = blockSize,
                                                FileContinuationToken = fileContinuationToken
                                            };

                var downloadBlockResponse = (DownloadBlockResponse)orgService.Execute(downloadBlockRequest);
                downloadBlockResponse.Data.CopyTo(fileBytes, blockOffset);
                blockOffset += blockSize;
            }

            return new FileData
            {
                FileBytes = fileBytes,
                MimeType = GetMimeType(initFileResponse.FileName),
                FileName = initFileResponse.FileName
            };
        }
    }

}
