package net.onebitsoftware.jwtsample;

import android.accounts.AccountManager;
import android.content.Context;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.support.v4.app.FragmentActivity;
import android.support.v4.app.FragmentTransaction;
import android.text.TextUtils;
import android.widget.Toast;

import com.android.volley.AuthFailureError;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.facebook.AccessToken;
import com.facebook.CallbackManager;
import com.facebook.FacebookCallback;
import com.facebook.FacebookException;
import com.facebook.FacebookSdk;
import com.facebook.login.LoginManager;
import com.facebook.login.LoginResult;
import com.google.android.gms.auth.GoogleAuthUtil;
import com.google.android.gms.auth.UserRecoverableAuthException;
import com.google.api.client.googleapis.extensions.android.gms.auth.GoogleAccountCredential;
import com.google.gson.Gson;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import de.greenrobot.event.EventBus;

public class MainActivity extends FragmentActivity {

    final private String CLIENT_ID = "519314902944-2u6u6o8trsmpa4kff1lr3h2hcaoanels.apps.googleusercontent.com";
    final private List<String> SCOPES = Arrays.asList("https://www.googleapis.com/auth/plus.login");
    private static final int REQUEST_ACCOUNT_PICKER = 100;
    private static final int REQUEST_AUTHORIZATION = 200;
    private GoogleAccountCredential mCredential;
    RequestQueue requestQueue;
    DataManager dataManager;
    CallbackManager callbackManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.fragment_container);

        //set dataManager
        dataManager = new DataManager(this);

        //set volley Q
        Context ct = this.getApplicationContext();
        requestQueue = RequestQueueSingleton.getInstance(ct).getRequestQueue();

        //register EventBus
        EventBus.getDefault().register(this);

        //decide what to show
        if (findViewById(R.id.fragment_container) != null) {
            LoginFragment loginFragment = new LoginFragment();
            loginFragment.setArguments(getIntent().getExtras());
            getSupportFragmentManager().beginTransaction().add(R.id.fragment_container, loginFragment).commit();
        }

        //initialize facebook sdk
        FacebookSdk.sdkInitialize(getApplicationContext());
        callbackManager = CallbackManager.Factory.create();

        //get access token if logged in with facebook account
        AccessToken at = AccessToken.getCurrentAccessToken();
        if(at != null) getAndSaveFacebookJwt(at.getToken());


        //init facebook login manager
        LoginManager.getInstance().registerCallback(callbackManager,
                new FacebookCallback<LoginResult>() {
                    @Override
                    public void onSuccess(LoginResult loginResult) {
                        // App code
                        String token = loginResult.getAccessToken().getToken();
                        getAndSaveFacebookJwt(token);
                    }

                    @Override
                    public void onCancel() {
                        // App code
                        String r = "13";
                    }

                    @Override
                    public void onError(FacebookException exception) {
                        // App code
                        String t = exception.toString();
                    }
                });
    }

    public void onEventMainThread(LoginEvent event) {
        User user = dataManager.getUser();
        //clear user credentials user if different token
        if(user.token != event.token) {
            user = new User();
            user.token = event.token;
            dataManager.setUser(user);
        }

        //call new fragment
        ProfileFragment profileFragment = new ProfileFragment();
        Bundle args = new Bundle();
        profileFragment.setArguments(args);
        FragmentTransaction transaction = getSupportFragmentManager().beginTransaction();
        transaction.replace(R.id.fragment_container, profileFragment);
        transaction.addToBackStack(null);
        transaction.commit();

        Toast.makeText(this, "Change fragment!", Toast.LENGTH_SHORT).show();
    }

    public void onEventMainThread(GoogleLoginEvent event) {
        // initiate a credential object with drive and plus.login scopes
        // cross identity is only available for tokens retrieved with plus.login
        mCredential = GoogleAccountCredential.usingOAuth2(this, null);
        // user needs to select an account, start account picker
        startActivityForResult(mCredential.newChooseAccountIntent(), REQUEST_ACCOUNT_PICKER);
    }

    @Override
    protected void onActivityResult(final int requestCode, final int resultCode, final Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        String accountName = data.getStringExtra(AccountManager.KEY_ACCOUNT_NAME);
        if (accountName != null) {
            mCredential.setSelectedAccountName(accountName);
            new RetrieveExchangeCodeAsyncTask().execute();
        }

        callbackManager.onActivityResult(requestCode, resultCode, data);
    }

    @Override
    public void onDestroy(){

        super.onDestroy();
        //EventBus.getDefault().unregister(this);
    }

    @Override
    protected void onStop() {
        //Log.w(TAG, "App stopped");

        super.onStop();
        //EventBus.getDefault().unregister(this);
    }

    /**
     * Retrieves the exchange code to be sent to the
     * server-side component of the app.
     */
    public class RetrieveExchangeCodeAsyncTask extends AsyncTask<Void, Boolean, String> {

        @Override
        protected String doInBackground(Void... params) {
            String scope = String.format("oauth2:server:client_id:%s:api_scope:%s",
                    CLIENT_ID, TextUtils.join(" ", SCOPES));
            try {
                String code = GoogleAuthUtil.getToken(MainActivity.this, mCredential.getSelectedAccountName(), scope);

                User user = dataManager.getUser();
                if(user.code == code){
                    //throw new UserRecoverableAuthException();
                }

                return code;
            } catch (UserRecoverableAuthException e) {
                Intent intent = e.getIntent();
                startActivityForResult(intent, REQUEST_AUTHORIZATION);
            } catch (Exception e) {
                e.printStackTrace();
            }
            return null;
        }

        @Override
        protected void onPostExecute(String code) {
            // exchange code with server-side to retrieve an additional
            // access token on the server-side.
            if(code != null) {
                getAndSaveGoogleJwt(code);
            }
        }
    }

    private void getAndSaveGoogleJwt(String code){

        String jwt = "";
        //http://localhost:28229/api/auth/login, "https://jwtsample.azurewebsites.net/api/auth/login", "http://10.0.2.2:28229/api/auth/local";
        //String url = "http://10.0.2.2:28229/api/auth/google";
        String url = "https://jwtsample.azurewebsites.net/api/auth/google";
        String redirectUri = "";

        JSONObject jsonBody = new JSONObject();
        try
        {
            jsonBody.put("clientId", CLIENT_ID);
            jsonBody.put("code", code);
            jsonBody.put("redirectUri", redirectUri);
        } catch (JSONException e) {
            e.printStackTrace();
        }

        final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.POST, url, jsonBody,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject jsonObject) {
                        System.out.println(jsonObject);
                        Gson gson = new Gson();
                        TokenResponse tokenResponse = gson.fromJson(jsonObject.toString(), TokenResponse.class);
                        if(tokenResponse.code == 200) {
                            EventBus.getDefault().post(new LoginEvent(tokenResponse.token));
                        }else if(tokenResponse.code == 400 || tokenResponse.code == 401){
                            System.out.println("Bad Req");
                        }else {
                            System.out.println("Error");
                        }
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        String err = new String(error.networkResponse.data);
                        System.out.println(error);
                    }
                }
        )
        {
            @Override
            public String getBodyContentType() {
                return "application/json";
                //return super.getBodyContentType();
            }

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> params = new HashMap<>();
                //params.put("Host", "localhost");
                return params;
            }
        }
        ;
        requestQueue.add(jsonObjectRequest);
    }

    private void getAndSaveFacebookJwt(String code){

        String jwt = "";
        String url;
        url = "https://jwtsample.azurewebsites.net/api/auth/facebook/mobile";


        JSONObject jsonBody = new JSONObject();
        try
        {
            jsonBody.put("access_token", code);
        } catch (JSONException e) {
            e.printStackTrace();
        }

        final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.POST, url, jsonBody,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject jsonObject) {
                        System.out.println(jsonObject);
                        Gson gson = new Gson();
                        TokenResponse tokenResponse = gson.fromJson(jsonObject.toString(), TokenResponse.class);
                        if(tokenResponse.code == 200) {
                            EventBus.getDefault().post(new LoginEvent(tokenResponse.token));
                        }else if(tokenResponse.code == 400 || tokenResponse.code == 401){
                            System.out.println("Bad Req");
                        }else {
                            System.out.println("Error");
                        }
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        String err = new String(error.networkResponse.data);
                        System.out.println(error);
                    }
                }
        )
        {
            @Override
            public String getBodyContentType() {
                return "application/json";
                //return super.getBodyContentType();
            }

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> params = new HashMap<>();
                //params.put("Host", "localhost");
                return params;
            }
        }
                ;
        requestQueue.add(jsonObjectRequest);
    }
}
