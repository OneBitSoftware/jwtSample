package net.onebitsoftware.jwtsample;

import android.content.Context;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import com.android.volley.AuthFailureError;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.ImageLoader;
import com.android.volley.toolbox.JsonObjectRequest;
//import com.facebook.login.widget.LoginButton;
import com.google.gson.Gson;

import org.json.JSONObject;

import java.util.HashMap;
import java.util.Map;

public class ProfileFragment extends Fragment {
    User user;
    RequestQueue requestQueue;
    ImageLoader imageLoader;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_profile, container, false);

        Context context = this.getActivity().getApplicationContext();
        requestQueue = RequestQueueSingleton.getInstance(context).getRequestQueue();
        imageLoader = RequestQueueSingleton.getInstance(context).getImageLoader();


        return view;
    }

    @Override
    public void onStart() {
        super.onStart();

        final Bundle args = getArguments();
        final DataManager dm = new DataManager(this.getActivity().getApplicationContext());

        if (args == null) return;

        user = dm.getUser();
        if (user == null) {
            Toast.makeText(this.getActivity(), "Error: Could not resolve user!", Toast.LENGTH_SHORT).show();
            return;
        }

        if (user.name != null && user.name != "") {
            updateArticleView(user);
            return;
        }

        String url = "https://jwtsample.azurewebsites.net/api/profile";
        //String url = "http://10.0.2.2:28229/api/profile";

        final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(url, null,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject jsonObject) {
                        System.out.println(jsonObject);
                        Gson gson = new Gson();
                        User userDetails = gson.fromJson(jsonObject.toString(), User.class);
                        user.name = userDetails.name;
                        user.email = userDetails.email;
                        user.picture = userDetails.picture;
                        dm.setUser(user);
                        updateArticleView(user);
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        String err = new String(error.networkResponse.data);
                        System.out.println(error);
                        updateArticleView(err, error.networkResponse.statusCode);
                    }
                }
        ) {
            //                @Override
            //                public String getBodyContentType() {
            //                    return "application/json";
            //                    //return super.getBodyContentType();
            //                }

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> params = new HashMap<>();
                params.put("Authorization", "Bearer " + user.token);
                //params.put("Host", "localhost");
                return params;
            }
        };
        requestQueue.add(jsonObjectRequest);
    }

    public void updateArticleView(User user) {
        TextView article = (TextView) getActivity().findViewById(R.id.fragment_profile_text);
        article.setText("Name: " + user.name + "\n\nEmail: " + user.email + "\n\nTOKEN: " + user.token);

        if(user.picture == null || user.picture == "") return;

        ImageView imageView = (ImageView)getActivity().findViewById(R.id.imageView);
        imageLoader.get(user.picture, ImageLoader.getImageListener(imageView, R.drawable.profile, R.drawable.profile));
    }

    public void updateArticleView(String error, int statusCode) {
        TextView article = (TextView) getActivity().findViewById(R.id.fragment_profile_text);
        article.setText("Request error: " + error + "\n\nStatus Code:" + statusCode);
    }
}
