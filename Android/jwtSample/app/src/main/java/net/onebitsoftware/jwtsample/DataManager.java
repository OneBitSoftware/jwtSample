package net.onebitsoftware.jwtsample;
import android.content.Context;
import com.google.gson.Gson;
import java.io.File;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.IOException;

public class DataManager {

    private final String DATA_STORE = "DataStore";
    FileOutputStream outputStream;
    Context context;

    public DataManager(Context context){
        this.context = context;
    }

    public void setUser(User user){

        Gson gson = new Gson();

        try {
            String json = gson.toJson(user);
            outputStream =  context.openFileOutput(DATA_STORE, Context.MODE_PRIVATE);
            outputStream.write(json.getBytes());
            outputStream.flush();
            outputStream.close();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public User getUser(){
        User user = new User();
        FileReader reader = null;
        Gson gson = new Gson();

        try {
            File file = new File(context.getFilesDir(), DATA_STORE);
            reader = new FileReader(file);
            char[] chars = new char[(int) file.length()];
            reader.read(chars);
            String content = new String(chars);
            reader.close();
            user = gson.fromJson(content, User.class);
        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            if(reader !=null){
                try {
                    reader.close();
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }
        return user;
    }
}
