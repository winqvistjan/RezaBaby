using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

using System.Drawing;
using System.Drawing.Drawing2D;
using ImageResizer;

namespace RezaBaby
{
    /// <summary>
    /// Class which perform paralle upload of azure blob
    /// </summary>
    public class AsyncBlockBlobUpload
    {
        private const int MaxBlockSize = 2097152; // Approx. 2MB chunk size

        // Aja Install-Package WindowsAxure.Storage
        // Lisää Microsoft.WindowsAzure.ServiceRuntime referenssi
            //this you specify from app.config         
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse("YourCloudStorageConnectionString");
            CloudStorageAccount storageAccount =
                     CloudStorageAccount.Parse(
                      CloudConfigurationManager.GetSetting("StorageConnectionString"));

            public List<BlobOperationStatus> UploadBlockBlobsInParallel(List<FileBlobNameMapper> fileBlobNameMapperList, string containerName)
            {
                //create list of blob operation status 
                List<BlobOperationStatus> blobOperationStatusList = new List<BlobOperationStatus>();

                //upload every file from list to blob in parallel (multitasking)
                Parallel.ForEach(fileBlobNameMapperList, fileBlobNameMapper =>
                {
                    try
                    {
                        string blobName = fileBlobNameMapper.BlobName;

                        //read file contents in byte array
                        byte[] fileContent = File.ReadAllBytes(fileBlobNameMapper.FilePath);
                        BlobOperationStatus blobStatus = null;
                        // Check if picture or video start
                        if (blobName.Contains(".mp4") || blobName.Contains(".MP4"))
                        {
                            blobStatus = UploadBlockBlobInternal(fileContent, containerName, blobName);
                        }
                        else
                        {
                            // Resize picture Start
                            byte[] newFileContent = null;
                            WebImage photo = new WebImage(fileContent);

                            if (photo != null)
                            {
                                if (photo.Height > photo.Width)
                                {
                                    // Portrait
                                    photo.Resize(width: 260, height: 346, preserveAspectRatio: true, preventEnlarge: false);
                                    newFileContent = photo.GetBytes();
                                }
                                else
                                {
                                    // Landscape
                                    photo.Resize(width: 260, height: 195, preserveAspectRatio: true, preventEnlarge: false);
                                    newFileContent = photo.GetBytes();
                                }
                            }
                            // Resize picture End

                            //call private method to actually perform upload of files to blob storage
                            blobStatus = UploadBlockBlobInternal(newFileContent, containerName, blobName);
                        }
                        // Check if picture or video end

                        //add the status of every blob upload operation to list.
                        blobOperationStatusList.Add(blobStatus);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                        //this.RedirectToAction("/ErrorManager/ServerError");
                        throw ex;
                    }
                });

                return blobOperationStatusList;
            }

            private BlobOperationStatus UploadBlockBlobInternal(byte[] fileContent, string containerName, string blobName)
            {
                BlobOperationStatus blobStatus = new BlobOperationStatus();
                try
                {
                    // Create the blob client.
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                    // Retrieve reference to container and create if not exists
                    CloudBlobContainer container = blobClient.GetContainerReference(containerName);
                    container.CreateIfNotExists();

                    // Retrieve reference to a blob and set the stream read and write size to minimum
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                    if (blobName.Contains(".mp4") || blobName.Contains(".MP4"))
                    {
                        blockBlob.Properties.ContentType = "video/mp4";
                    }
                    else
                    {
                        blockBlob.Properties.ContentType = "image/jpeg";
                    }
                    
                    blockBlob.StreamWriteSizeInBytes = 1048576;
                    blockBlob.StreamMinimumReadSizeInBytes = 1048576;
                    blockBlob.Metadata.Add("username", "kunal");

                    //set the blob upload timeout and retry strategy
                    BlobRequestOptions options = new BlobRequestOptions();
                    options.ServerTimeout = new TimeSpan(0, 180, 0);
                    options.RetryPolicy = new ExponentialRetry(TimeSpan.Zero, 20);

                    //get the file blocks of 2MB size each and perform upload of each block
                    HashSet<string> blocklist = new HashSet<string>();
                    List<FileBlock> bloksT = GetFileBlocks(fileContent).ToList();
                    foreach (FileBlock block in GetFileBlocks(fileContent))
                    {
                        blockBlob.PutBlock(
                            block.Id,
                            new MemoryStream(block.Content, true), null,
                            null, options, null
                            );

                        blocklist.Add(block.Id);

                    }
                    //commit the blocks that are uploaded in above loop
                    blockBlob.PutBlockList(blocklist, null, options, null);

                    //set cache control property for the uploaded blob
                    //5 minutes of caching on client side. you can change it as per your requirement.
                    blockBlob.Properties.CacheControl = "max-age=300, must-revalidate";
                    blockBlob.SetProperties();


                    //set the status of operation of blob upload as succeeded as there is not exception
                    blobStatus.BlobUri = blockBlob.Uri;
                    blobStatus.Name = blockBlob.Name;
                    blobStatus.OperationStatus = OperationStatus.Succeded;

                    return blobStatus;
                }
                catch (Exception ex)
                {
                    //set the status of blob upload as failed along with exception message
                    blobStatus.Name = blobName;
                    blobStatus.OperationStatus = OperationStatus.Failed;
                    blobStatus.ExceptionDetails = ex;
                    return blobStatus;
                }
            }

            private IEnumerable<FileBlock> GetFileBlocks(byte[] fileContent)
            {
                HashSet<FileBlock> hashSet = new HashSet<FileBlock>();
                if (fileContent.Length == 0)
                    return new HashSet<FileBlock>();

                int blockId = 0;
                int ix = 0;

                int currentBlockSize = MaxBlockSize;

                while (currentBlockSize == MaxBlockSize)
                {
                    if ((ix + currentBlockSize) > fileContent.Length)
                        currentBlockSize = fileContent.Length - ix;

                    byte[] chunk = new byte[currentBlockSize];
                    Array.Copy(fileContent, ix, chunk, 0, currentBlockSize);

                    hashSet.Add(
                        new FileBlock()
                        {
                            Content = chunk,
                            Id = Convert.ToBase64String(System.BitConverter.GetBytes(blockId))
                        });

                    ix += currentBlockSize;
                    blockId++;
                }

                return hashSet;
            }
        }

        internal class FileBlock
        {
            public string Id
            {
                get;
                set;
            }

            public byte[] Content
            {
                get;
                set;
            }
        }
}