# jwtSample
Used for internal presentation.
Aim to introduce the JSON Web Token standard.
Aim to connect two web client and android app with web api and authenticate the clients against the api with JSON Web Token (JWT).
Do not use in production. The code is made for the purposes of the presentation and it is not bug free.

Presentation notes:

JSON Web Tokens

- JSON Web Tokens are an open, industry standard RFC 7519 method for representing claims securely between two parties. - http://jwt.io/
- Can replace cookies
- Why? Can do auth between different (cors)DEVICES. 
- Used in most oauth apis - https://developers.google.com/apis-explorer/#p/, 
- Run over SSL coz Man in the middle.
- Language and platform independent. HTTP bound, Websockets custom implementation
- https://developers.google.com/identity/protocols/OAuth2?hl=en - OAuth2.0 process
- The AIM:

      +-----------------+               +------------------+
      |                 |               |                  |
      |                 +---------------> OAuth2.0 Provider|
      |    WebApi       |               |                  |
      |                 | <-------------+                  |
      +--+-----------+--+               +------------------+
         |           |                                      
         |           |                                      
         |           |                                      
         |           |                                      
         |           |                                      
         |           |                                      
         |           |                                      
         |           |                                      
         v           v                                                                           
+------------+  +-------------+                             
|            |  |             |                             
| Android    |  | SPA         |                             
|            |  |             |                             
|            |  |             |                             
+------------+  +-------------+       