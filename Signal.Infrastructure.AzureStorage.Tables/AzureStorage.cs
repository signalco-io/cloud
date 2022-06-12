using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Signal.Core.Contacts;
using Signal.Core.Entities;
using Signal.Core.Newsletter;
using Signal.Core.Sharing;
using Signal.Core.Storage;
using Signal.Core.Storage.Blobs;
using Signal.Core.Users;

namespace Signal.Infrastructure.AzureStorage.Tables;

internal class AzureStorage : IAzureStorage
{
    private readonly IAzureStorageDao dao;
    private readonly IAzureStorageClientFactory clientFactory;


    public AzureStorage(
        IAzureStorageDao dao,
        IAzureStorageClientFactory clientFactory)
    {
        this.dao = dao ?? throw new ArgumentNullException(nameof(dao));
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    }
        

    public async Task UpsertAsync(IEntity entity, CancellationToken cancellationToken = default)
    {
        await this.WithClientAsync(
            ItemTableNames.Entities,
            c => c.UpsertEntityAsync(AzureEntity.FromEntity(entity), cancellationToken: cancellationToken),
            cancellationToken);
    }

    public async Task UpsertAsync(IContactPointer contact, CancellationToken cancellationToken = default)
    {
        await this.WithClientAsync(
            ItemTableNames.Contacts,
            c => c.UpsertEntityAsync(AzureContact.FromContactPointer(contact), cancellationToken: cancellationToken),
            cancellationToken);
    }

    public async Task UpsertAsync(IContact contact, CancellationToken cancellationToken = default)
    {
        await this.WithClientAsync(
            ItemTableNames.Contacts,
            c => c.UpsertEntityAsync(AzureContact.FromContact(contact), cancellationToken: cancellationToken),
            cancellationToken);
    }

    public async Task UpsertAsync(IContactHistoryItem item, CancellationToken cancellationToken = default)
    {
        await this.WithClientAsync(
            ItemTableNames.ContactsHistory,
            c => c.UpsertEntityAsync(AzureContactHistoryItem.FromContactHistoryItem(item), cancellationToken: cancellationToken),
            cancellationToken);
    }

    public async Task UpsertAsync(IUserAssignedEntity userAssignment, CancellationToken cancellationToken = default)
    {
        await this.WithClientAsync(
            ItemTableNames.UserAssignedEntity,
            c => c.UpsertEntityAsync(AzureUserAssignedEntitiesTableEntry.From(userAssignment), cancellationToken: cancellationToken),
            cancellationToken);
    }

    public async Task UpsertAsync(IUser user, CancellationToken cancellationToken = default)
    {
        await this.WithClientAsync(
            ItemTableNames.Users,
            c => c.UpsertEntityAsync(AzureUser.FromUser(user), cancellationToken: cancellationToken),
            cancellationToken);
    }

    public async Task UpsertAsync(INewsletterSubscription subscription, CancellationToken cancellationToken = default)
    {
        await this.WithClientAsync(
            ItemTableNames.Website.Newsletter,
            client => client.UpsertEntityAsync(new AzureNewsletterSubscription(subscription.Email),
                cancellationToken: cancellationToken),
            cancellationToken);
    }

    public async Task RemoveEntityAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var entity = await this.dao.GetAsync(id, cancellationToken);
        if (entity != null)
        {
            await this.WithClientAsync(ItemTableNames.Entities,
                client => client.DeleteEntityAsync(
                    entity.Type.ToString(), 
                    entity.Id,
                    cancellationToken: cancellationToken),
                cancellationToken);
        }
    }
    
    public async Task AppendToFileAsync(string directory, string fileName, Stream data, CancellationToken cancellationToken)
    {
        var client = await this.clientFactory.GetAppendBlobClientAsync(
            BlobContainerNames.StationLogs, 
            $"{directory.Replace("\\", "/")}/{fileName}",
            cancellationToken);

        // TODO: Handle data sizes over 4MB
        await client.AppendBlockAsync(data, cancellationToken: cancellationToken);
    }

    private async Task WithClientAsync(string tableName, Func<TableClient, Task> action, CancellationToken cancellationToken = default)
    {
        var client = await this.clientFactory.GetTableClientAsync(tableName, cancellationToken);
        await action(client);
    }
}