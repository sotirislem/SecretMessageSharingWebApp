# SecretMessageSharingWebApp

A web app that allows you to create encrypted messages that can be securely shared with a unique recipient. \
Each message is encrypted/decrypted locally, on browser, by using the AES-256 algorithm in GCM mode. \
Message can also include a file attachment of any type as long as its size does not exceed ~1.2MB. \
Remote server is only used to store the encrypted message, so it can be later retrieved by a third person. \
Sender has the ability to receive a delivery notification on his browser once his message is read by a recipient. \
The encryption key which is used to encrypt/decrypt a message is locally created on user's browser and never gets exposed to server. \
Every generated message is removed from server upon its retrieval. Also, unread messages are automatically destroyed 1 hour after their creation timestamp. \
For even more extra security, an OTP mechanism can seal the message further, allowing successful recovery only after the recipient provides a valid OTP that will be delivered to their email address.

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
TODO: Add updated interface images