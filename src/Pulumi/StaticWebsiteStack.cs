using System;
using System.IO;
using System.Linq;
using MimeTypes;
using Pulumi.Azure.Constants;
using Pulumi.Azure.Core;
using Pulumi.Azure.Storage;
using Pulumi.Azure.Storage.Inputs;

namespace Pulumi.Azure.StaticWebsite
{
    class StaticWebsiteStack : Stack
    {
        public StaticWebsiteStack()
        {
            // Create an Azure Resource Group
            var resourceGroup = new ResourceGroup("stef-rg-static-websites", new ResourceGroupArgs
            {
                Location = "West Europe"
            });

            // Create an Azure Storage Account
            var storageAccount = new Account("blazorhandlebars", new AccountArgs
            {
                ResourceGroupName = resourceGroup.Name,
                EnableHttpsTrafficOnly = true,
                AccountReplicationType = StorageAccountReplicationTypes.LRS,
                AccountTier = StorageAccountTiers.Standard,
                AccountKind = StorageAccountKinds.StorageV2,
                StaticWebsite = new AccountStaticWebsiteArgs
                {
                    IndexDocument = "index.html",
                    // Error404Document = "404.html"
                }
            });

            // Upload the files
            string currentDirectory = Directory.GetCurrentDirectory();
            var rootDirectory = Directory.GetParent(Directory.GetParent(currentDirectory).FullName);
            string publishDirectory = Path.Combine(rootDirectory.FullName, "docs-temp", "wwwroot");

            var files = Directory.EnumerateFiles(publishDirectory, "*.*", SearchOption.AllDirectories)
                .Select(x => new
                {
                    path = x,
                    name = x.Remove(0, publishDirectory.Length + 1).Replace(Path.PathSeparator, '/'),
                    ext = new FileInfo(x).Extension
                })
                .Where(x => !x.name.StartsWith('.'))
                ;

            foreach (var file in files)
            {
                var uploadedFile = new Blob(file.name, new BlobArgs
                {
                    Name = file.name,
                    StorageAccountName = storageAccount.Name,
                    StorageContainerName = "$web",
                    Type = "Block",
                    Source = new FileAsset(file.path),
                    ContentType = MimeTypeMap.GetMimeType(file.ext)
                });
            }

            // Export the Web address string for the storage account
            StaticEndpoint = storageAccount.PrimaryWebEndpoint;
        }

        [Output]
        public Output<string> StaticEndpoint { get; set; }
    }
}
