using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VkParse
{
    public class User
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string DirectoryPath { get; set; }
        public List<Photos> Photos { get; set; } = new List<Photos>();
    }

    public class Photos
    {
        public string Uri { get; set; }
    }

    internal class ParseImages
    {
        string MessageForler;
        string OutputFolder;
        Logs Logs;
        string Token;
        private List<User> Users { get; set; }
        private List<string> MissId { get; set; }

        public ParseImages(string token, string messageFolderPath, string outputFolderPath, List<string> missIds)
        {
            MessageForler = messageFolderPath;
            OutputFolder = outputFolderPath;
            Users = new List<User>();
            Logs = new Logs();
            MissId = missIds;
            Token = token;
        }

        public void Start()
        {
            Users = GetUsers(MessageForler);
            Logs.SendLog("Пользователи получены");

            Users = GetNames(Users);
            Logs.SendLog("Имена получены");

            Users = ParseUrls(Users);
            Logs.SendLog("Ссылки спарсены");

            DownloadImages(Users, OutputFolder);
            Logs.SendLog("Фото скачаны");
        }

        private void DownloadImages(List<User> users, string outputFolder)
        {
            WebClient web;
            FileInfo fileInfo;
            DirectoryInfo directoryInfo;

            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Photos.Count == 0)
                    continue;

                string folder = outputFolder + $"\\{users[i].Name}";

                directoryInfo = new DirectoryInfo(folder);
                if (!directoryInfo.Exists)
                    directoryInfo.Create();

                for (int j = 0; j < users[i].Photos.Count; j++)
                {
                    web = new WebClient();
                    fileInfo = new FileInfo($"{folder}\\{j}.png");

                    if (fileInfo.Exists)
                    {
                        Logs.SendLog($"{users[i].Name} - {j} - уже");
                        continue;
                    }

                    try
                    {
                        web.DownloadFile(users[i].Photos[j].Uri, $"{folder}\\{j}.png");
                    }
                    catch
                    {
                        Logs.SendLog($"{users[i].Name} - {j} - ошибка");
                        continue;
                    }

                    fileInfo = new FileInfo($"{folder}\\{j}.png");
                    if (fileInfo.Exists)
                    {
                        Logs.SendLog($"{users[i].Name} - {j} - да");
                    }
                    else
                    {
                        Logs.SendLog($"{users[i].Name} - {j} - нет");
                    }
                }
            }
        }

        private List<User> ParseUrls(List<User> users)
        {
            for (int i = 0; i < users.Count; i++)///пользователи
            {
                var messages = new DirectoryInfo(users[i].DirectoryPath).GetFiles();

                List<string> vs = new List<string>();

                for (int j = 0; j < messages.Length; j++)///сообщения пользователя
                {
                    using (StreamReader st = new StreamReader(messages[j].FullName, Encoding.GetEncoding("windows-1251")))
                    {
                        vs = GetUris(st.ReadToEnd());

                        for (int z = 0; z < vs.Count; z++)
                        {
                            users[i].Photos.Add(
                                new Photos
                                {
                                    Uri = vs[z]
                                }
                                );
                        }

                        vs.Clear();
                    }
                }
            }

            return users;
        }

        /// <summary>
        /// Получение всех ссылок из текста
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private List<string> GetUris(string text)
        {
            //text = text.Replace("\n", "").Replace("\t", "");
            List<string> uris = new List<string>();

            Regex regex = new Regex(@"https:\/\/sun(.+?).com\/sun(.+?)\/(.+?)\/(.+?)\/(.+?)\/(.+?)\.(.+?)?size=(.+?)&quality=(.+?)&type=album");

            MatchCollection matchCollection = regex.Matches(text);

            foreach (Match match in matchCollection)
            {
                uris.Add(match.Value.Replace("'>", ""));
            }
            
            uris = uris.Distinct().ToList();

            return uris;
        }

        /// <summary>
        /// получение списка с именами
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        private List<User> GetNames(List<User> users)
        {
            for (int i = 0; i < users.Count; i++)
            {
                users[i].Name = GetNameFromId(users[i].UserId);

                if (users[i].Name == "DELETED ")
                    users[i].Name = "DELETED" + users[i].UserId;
            }

            return users;
        }


        /// <summary>
        /// Получение имени по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetNameFromId(string id)
        {
            string ur = $"https://api.vk.com/method/users.get?user_id={id}&access_token={Token}&v=5.131";

            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            var a = webClient.DownloadString(ur);
            Models.Response userPage = System.Text.Json.JsonSerializer.Deserialize<Models.Response>(a);

            if (userPage.response.Length == 0)
            {
                return id;
            }
            else
                return userPage.response[0].first_name + " " + userPage.response[0].last_name;
        }

        /// <summary>
        /// Получение всех пользователей из папки с сообщениями
        /// </summary>
        /// <returns></returns>
        private List<User> GetUsers(string messFolder)
        {
            DirectoryInfo fileInfo = new DirectoryInfo(messFolder);
            var a = fileInfo.GetDirectories();

            List<User> users = new List<User>();

            for (int i = 0; i < a.Length; i++)
            {
                bool f = true;
                for (int j = 0; j < MissId.Count; j++)
                {
                    if (a[i].Name == MissId[j])
                    {
                        f = false;
                        continue;
                    }
                }

                if (f)
                    users.Add(new User { DirectoryPath = a[i].FullName, UserId = a[i].Name });
            }

            return users;
        }
    }
}
