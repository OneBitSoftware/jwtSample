package net.onebitsoftware.jwtsample;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;

import com.android.volley.AuthFailureError;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.facebook.CallbackManager;
import com.facebook.FacebookCallback;
import com.facebook.FacebookException;
import com.facebook.FacebookSdk;
import com.facebook.login.LoginResult;
import com.facebook.login.widget.LoginButton;
import com.google.gson.Gson;

import org.json.JSONException;
import org.json.JSONObject;

import java.security.Provider;
import java.util.HashMap;
import java.util.Map;

import de.greenrobot.event.EventBus;

public class LoginFragment extends Fragment {

    Button loginButton;
    Button googleLoginButton;
    EditText nameInput;
    EditText passwordInput;
    RequestQueue requestQueue;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        View rootView = inflater.inflate(R.layout.fragment_login, container, false);
        Context context = this.getActivity().getApplicationContext();

        //get volley Q
        requestQueue = RequestQueueSingleton.getInstance(context).getRequestQueue();

        loginButton = (Button) rootView.findViewById(R.id.login);
        googleLoginButton = (Button) rootView.findViewById(R.id.googleLogin);
        nameInput = (EditText) rootView.findViewById(R.id.name);
        passwordInput = (EditText) rootView.findViewById(R.id.password);
        LoginButton facebookLoginButton = (LoginButton) rootView.findViewById(R.id.login_button);
        facebookLoginButton.setReadPermissions("public_profile", "email");
        //facebookLoginButton.setFragment(this);

        loginButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View view) {
                loginUser();
            }
        });

        googleLoginButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View view) {
                EventBus.getDefault().post(new GoogleLoginEvent());
            }
        });



        return rootView;
    }


    private void loginUser() {

        //http://localhost:28229/api/auth/login, "https://jwtsample.azurewebsites.net/api/auth/login", "http://10.0.2.2:28229/api/auth/local";
        //String url = "http://10.0.2.2:28229/api/auth/local";
        String url = "https://jwtsample.azurewebsites.net/api/auth/local";

        JSONObject jsonBody = new JSONObject();
        try {
            jsonBody.put("name", nameInput.getText());
            jsonBody.put("password", passwordInput.getText());
        } catch (JSONException e) {
            e.printStackTrace();
        }

        final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.POST, url, jsonBody,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject jsonObject) {
                        System.out.println(jsonObject);
                        Gson gson = new Gson();
                        User user = gson.fromJson(jsonObject.toString(), User.class);
                        EventBus.getDefault().post(new LoginEvent(user.token));
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

//            @Override
//            public Map<String, String> getHeaders() throws AuthFailureError {
//                Map<String, String> params = new HashMap<>();
//                params.put("Host", "localhost");
//                return params;
//            }
        }
        ;
        requestQueue.add(jsonObjectRequest);
    }
}

