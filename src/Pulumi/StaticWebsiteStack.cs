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
            bool newResourceGroup = false;
            ResourceGroup resourceGroup;
            if (newResourceGroup)
            {
                // Create an Azure Resource Group
                resourceGroup = new ResourceGroup("stef-rg-static-websites", new ResourceGroupArgs
                {
                    Location = "West Europe"
                });
            }
            else
            {
                // Use existing an Azure Resource Group
                resourceGroup = ResourceGroup.Get("stef-rg-static-websites", "/subscriptions/ae9255af-d099-4cdc-90a7-241ccb29df68/resourceGroups/stef-rg-static-websites");
            }

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
                    //Error404Document = "404.html" // https://github.com/pulumi/pulumi-azure/issues/512
                }
            });

            // Upload the files from local to azure storage account
            string sourceFolder = Path.Combine("docs-temp", "wwwroot");
            string currentDirectory = Directory.GetCurrentDirectory();
            var rootDirectory = Directory.GetParent(Directory.GetParent(currentDirectory).FullName);
            string publishDirectory = Path.Combine(rootDirectory.FullName, sourceFolder);

            var files = Directory.EnumerateFiles(publishDirectory, "*.*", SearchOption.AllDirectories)
                .Select(path => new
                {
                    path, // The full source path
                    name = path.Remove(0, publishDirectory.Length + 1).Replace(Path.PathSeparator, '/'), // Make the name Azure Storage comatible
                    info = new FileInfo(path)
                })
                .Where(file => file.info.Length > 0) // https://github.com/pulumi/pulumi-azure/issues/544
                ;

            foreach (var file in files)
            {
                var uploadedFile = new Blob(file.name, new BlobArgs
                {
                    Name = file.name,
                    StorageAccountName = storageAccount.Name,
                    StorageContainerName = "$web",
                    Type = BlobTypes.Block,
                    Source = new FileAsset(file.path),
                    ContentType = MimeTypeMap.GetMimeType(file.info.Extension)
                });
            }

            // Export the Web address string for the storage account
            StaticEndpoint = storageAccount.PrimaryWebEndpoint;
        }

        [Output]
        public Output<string> StaticEndpoint { get; set; }
    }
}
