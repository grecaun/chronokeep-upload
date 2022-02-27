using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace results_uploader.Objects
{
    public class ResultsAPI
    {
        private string type, url, nickname, auth_token;

        public ResultsAPI()
        {
            type = Constants.ResultsAPI.CHRONOKEEP;
            url = Constants.ResultsAPI.CHRONOKEEP_URL;
            auth_token = "";
            nickname = "";
        }

        public ResultsAPI(string type, string url, string nickname, string auth_token)
        {
            this.type = type;
            this.url = url;
            this.nickname = nickname;
            this.auth_token = auth_token;
        }

        public string Type { get => type; set => type = value; }
        public string URL { get => url; set => url = value; }
        public string Nickname { get => nickname; set => nickname = value; }
        public string AuthToken { get => auth_token; set => auth_token = value; }
    }
}
