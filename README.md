Vk Parse Photo From Data Archieve

Using this project you can get all the photos from the archive vkontakte data.

//How to use

Create an instance of the ParseImages class and pass 4 parameters: the vkontakte token, the path to the dialog folder, the output directory, a string type list with exclusive ids.

```C#
ParseImages parseImages = new ParseImages("vkAnyAccessToken", 
                @"C:\Users\oliso\Desktop\arh2\messages", 
                @"C:\Users\oliso\Desktop\out", 
                new List<string> { "100000001" });
```
Next, call the Start() method on the previously created instance
