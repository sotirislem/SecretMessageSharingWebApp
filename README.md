# SecretMessageSharingWebApp

A web app that allows you to create encrypted messages that can be securely shared with a unique recipient. \
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
* Stanford JavaScript Crypto Library (for local in-browser encryption)

### Publish URL:
https://secret-message-sharing-web-app.azurewebsites.net/

## App preview
![app-preview-image1](https://user-images.githubusercontent.com/10964246/187239788-700feff0-9ca9-45e1-a5ae-a83b314f2c46.png) \
![app-preview-image2](https://user-images.githubusercontent.com/10964246/187239801-8e945f8d-d01d-4df0-8d39-11120335a282.png) \
![app-preview-image3](https://user-images.githubusercontent.com/10964246/187239808-024fb3de-f08b-40bb-a808-48249c38382b.png)
