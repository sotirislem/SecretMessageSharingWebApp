# SecretMessageSharingWebApp

An app that allows you to create encrypted messages that can be securely shared with a unique recipient. \
Each message is encrypted/decrypted locally, on browser, by using the AES-256 algorithm in GCM mode. \
Remote server is only used to store the encrypted message, so it can be later retrieved by a third person. \
Sender has the ability to receive a delivery notification on his browser once his message is read by a recipient. \
The encryption key which is used to encrypt/decrypt a message is locally created on user's browser and never gets exposed to server. \
Every generated message is removed from server upon its retrieval. Also, unread messages are automatically destroyed 1 hour after their creation timestamp.

### Made with:
* ASP.NET Core 6
* Entity Framework Core
* Angular 13

### Core Libraries used:
* FastEndpoints (instead of MVC Controllers)
* SignalR (for delivery notifications)
* Stanford Javascript Crypto Library (for local in-browser encryption)

## App preview
![app-preview-image1](https://user-images.githubusercontent.com/10964246/182240664-ba143c29-b63c-46cc-ae9a-c589de51fb70.png)
![app-preview-image2](https://user-images.githubusercontent.com/10964246/182240670-1941290d-7db3-4dc1-a494-93c5d0521eff.png)
![app-preview-image3](https://user-images.githubusercontent.com/10964246/182240672-fc5dd921-9f96-4820-b03c-d5926c60030f.png)
