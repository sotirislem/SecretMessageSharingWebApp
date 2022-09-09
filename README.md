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
![Screenshot 2022-09-09 170543](https://user-images.githubusercontent.com/10964246/189373567-09c32895-dc89-4c9d-99b9-8ea36631558e.png)
![Screenshot 2022-09-09 170649](https://user-images.githubusercontent.com/10964246/189373571-aafa18d9-23ba-47ae-87bb-f38107a69eca.png)
![Screenshot 2022-09-09 170713](https://user-images.githubusercontent.com/10964246/189371661-a0b28e65-e8cd-44ca-8dd6-e6e5dad4ca14.png)
![Screenshot 2022-09-09 170725](https://user-images.githubusercontent.com/10964246/189371672-9f71bddd-37f7-46ed-9023-4d14895eecb1.png)
![Screenshot 2022-09-09 170749](https://user-images.githubusercontent.com/10964246/189371663-26a9ec9f-b410-4fad-b85b-94e8e1e554b2.png) \
![Screenshot 2022-09-09 170837](https://user-images.githubusercontent.com/10964246/189371673-b08d9c3b-ac4a-493b-8873-1583c66b3e5f.png) \
![Screenshot 2022-09-09 171145](https://user-images.githubusercontent.com/10964246/189371665-06169fd9-d596-4af1-9d08-86c4053c4878.png)
