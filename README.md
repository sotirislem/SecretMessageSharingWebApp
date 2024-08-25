# SecretMessageSharingWebApp

A web app that allows you to create encrypted messages that can be securely shared with a unique recipient. \
Each message is encrypted/decrypted locally, on browser, by using the AES-256 algorithm in GCM mode. \
Message can also include a file attachment of any type as long as its size does not exceed ~1.2MB. \
Remote server is only used to store the encrypted message, so it can be later retrieved by a third person. \
Sender has the ability to receive a delivery notification on his browser once his message is read by a recipient. \
The encryption key which is used to encrypt/decrypt a message is locally created on user's browser and never gets exposed to server. \
Every generated message is removed from server upon its retrieval. Also, unread messages are automatically destroyed 1 hour after their creation timestamp. \
For even more security, an OTP mechanism can seal the message further, allowing successful recovery only after the recipient provides a valid OTP that will be delivered to their email address.

### Made with:
* Angular 14
* .NET 8
* Entity Framework Core

### Libraries used:
* FastEndpoints (instead of MVC Controllers)
* SignalR (for delivery notifications)
* Stanford JavaScript Crypto Library (for local in-browser encryption)

### Hosting:
The App is hosted on Azure by using Azure App Service, whereas, uses Azure Cosmos DB for data storage.
OTP codes are delivered by using the SendGrid email delivery platform.
#### Publish URL: https://secret-message-sharing-web-app.azurewebsites.net/

## App preview
![1](https://user-images.githubusercontent.com/10964246/201693909-a9cd86ad-2f0a-404c-a03a-001a1849877e.png) \
![2](https://user-images.githubusercontent.com/10964246/201693921-7980e653-d204-4095-8d78-2ee57a577e36.png) \
![3](https://user-images.githubusercontent.com/10964246/201693927-9d948265-99ba-40d5-a808-2f3495ad20c3.png) \
![4](https://user-images.githubusercontent.com/10964246/201693931-f0fc4d27-df73-4b68-902c-b105ae78ab2d.png) \
![5](https://user-images.githubusercontent.com/10964246/201693950-f5f85347-9096-4c8a-946c-064e483856a6.png) \
![6](https://user-images.githubusercontent.com/10964246/201693956-f319cc50-b8a4-4f0c-a3a3-6db9c1128fd4.png) \
![7](https://user-images.githubusercontent.com/10964246/201693969-3ce97fe1-786e-477b-b036-eda357ea62d5.png)
