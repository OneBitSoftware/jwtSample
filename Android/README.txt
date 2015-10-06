1. Android APP should posses certificate
	- Generate cretificate form Android studio Build -> Generate Signed APK
	- Assign it to the app on run -> hover the app folder -> open context menu -> Open Module Settings. On Signing add youyr sertificate
	androiddebugkey|android|C:\Users\Velkata\.android\debug.keystore|android
2. Register "Client ID for Android application" in https://console.developers.google.com/project/blabla...
3. Get the Certificate fingerprint (SHA1) by using the jdk/bin/keytool like:
	- keytool -exportcert -alias androiddebugkey -keystore ~/.android/debug.keystore -list -v
	For Facebook: keytool -exportcert -alias androiddebugkey -keystore ~/.android/debug.keystore | openssl sha1 -binary | openssl base64


Facebook info:
Getting Started Android SDK - https://developers.facebook.com/docs/android/getting-started#app_id
Facebook Login - https://developers.facebook.com/docs/facebook-login/android/v2.4