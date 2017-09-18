
using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Text;

namespace PBIWebApp
{
    /* NOTE: This sample is to illustrate how to authenticate a Power BI web app. 
    * In a production application, you should provide appropriate exception handling and refactor authentication settings into 
    * a configuration. Authentication settings are hard-coded in the sample to make it easier to follow the flow of authentication. */
    public partial class _Default : Page
    {
        public const string authResultString = "authResult";
        public AuthenticationResult authResult { get; set; }
        string baseUri = Properties.Settings.Default.PowerBiDataset;

        protected void Page_Load(object sender, EventArgs e)
        {

            //Test for AuthenticationResult
            if (Session[authResultString] != null)
            {
                //Get the authentication result from the session
                authResult = (AuthenticationResult)Session[authResultString];

                //Show Power BI Panel
                signInStatus.Visible = true;
                signInButton.Visible = false;

                //Set user and token from authentication result
                userLabel.Text = authResult.UserInfo.DisplayableId;
                accessTokenTextbox.Text = authResult.AccessToken;
            }
        }

        protected void signInButton_Click(object sender, EventArgs e)
        {
            //Create a query string
            //Create a sign-in NameValueCollection for query string
            var @params = new NameValueCollection
            {
                //Azure AD will return an authorization code. 
                //See the Redirect class to see how "code" is used to AcquireTokenByAuthorizationCode
                {"response_type", "code"},

                //Client ID is used by the application to identify themselves to the users that they are requesting permissions from. 
                //You get the client id when you register your Azure app.
                {"client_id", Properties.Settings.Default.ClientID},

                //Resource uri to the Power BI resource to be authorized
                {"resource", Properties.Settings.Default.PowerBiAPI},

                //After user authenticates, Azure AD will redirect back to the web app
                {"redirect_uri", "http://localhost:13526/Redirect"}
            };

            //Create sign-in query string
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add(@params);

            //Redirect authority
            //Authority Uri is an Azure resource that takes a client id to get an Access token
            string authorityUri = Properties.Settings.Default.AADAuthorityUri;
            var authUri = String.Format("{0}?{1}", authorityUri, queryString);
            Response.Redirect(authUri);
        }

        private  void   GetToken()
        {
            List<KeyValuePair<string, string>> vals = new List<KeyValuePair<string, string>>();
            vals.Add(new KeyValuePair<string, string>("grant_type", "password"));
            vals.Add(new KeyValuePair<string, string>("scope", "openid"));
            vals.Add(new KeyValuePair<string, string>("resource", "https://analysis.windows.net/powerbi/api"));
            vals.Add(new KeyValuePair<string, string>("client_id", "6334f62b-b4c5-4784-8719-8d8c62905bf1"));
            vals.Add(new KeyValuePair<string, string>("client_secret", "M5PJ3hQTEwQHXmK3JvKxqaeqyBBNPbEpbo4siFD8z/g="));
            vals.Add(new KeyValuePair<string, string>("username", "admin@jmfinancial.onmicrosoft.com"));
            vals.Add(new KeyValuePair<string, string>("password", "O365!O365@O118"));
            string TenantId = "86ba6754-ade1-48ce-ad3a-7388495377b6";
            string url = string.Format("https://login.windows.net/{0}/oauth2/token", TenantId);
            HttpClient hc = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(vals);
            HttpResponseMessage hrm = hc.PostAsync(url, content).Result;
            string responseData = "";
            if (hrm.IsSuccessStatusCode)
            {
                //Stream data = await hrm.Content.ReadAsStreamAsync();
                //using (StreamReader reader = new StreamReader(data, Encoding.UTF8))
                //{
                //    responseData = reader.ReadToEnd();
                //}
            }
            var Token = JsonConvert.DeserializeObject<AccessToken>(responseData);
        }

        protected void getDashboardsButton_Click(object sender, EventArgs e)
        {
            string responseContent = string.Empty;
             GetToken();
            //Code by swapnil

            /////

            //Configure dashboards request
            System.Net.WebRequest request = System.Net.WebRequest.Create(String.Format("{0}dashboards", baseUri)) as System.Net.HttpWebRequest;
            request.Method = "GET";
            request.ContentLength = 0;
            request.Headers.Add("Authorization", String.Format("Bearer {0}", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkhIQnlLVS0wRHFBcU1aaDZaRlBkMlZXYU90ZyIsImtpZCI6IkhIQnlLVS0wRHFBcU1aaDZaRlBkMlZXYU90ZyJ9.eyJhdWQiOiJodHRwczovL2FuYWx5c2lzLndpbmRvd3MubmV0L3Bvd2VyYmkvYXBpIiwiaXNzIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvODZiYTY3NTQtYWRlMS00OGNlLWFkM2EtNzM4ODQ5NTM3N2I2LyIsImlhdCI6MTUwNTc0ODczOCwibmJmIjoxNTA1NzQ4NzM4LCJleHAiOjE1MDU3NTI2MzgsImFjciI6IjEiLCJhaW8iOiJZMlZnWUdqNE9WOXpxOG1hT082SzFGbjFCMCtjYnZzOXIwR3ZZbk9TYVlHanQ2WlJEZzhBIiwiYW1yIjpbInB3ZCJdLCJhcHBpZCI6IjYzMzRmNjJiLWI0YzUtNDc4NC04NzE5LThkOGM2MjkwNWJmMSIsImFwcGlkYWNyIjoiMSIsImVfZXhwIjoyNjI4MDAsImZhbWlseV9uYW1lIjoiQWRtaW4iLCJnaXZlbl9uYW1lIjoiTWFpbCIsImlwYWRkciI6IjEyNS45OS41Ny4yMjIiLCJuYW1lIjoiTWFpbCBBZG1pbiIsIm9pZCI6ImNmN2YzM2JkLTgxNTEtNDEzNS1iMmYzLTA4NDE1ODExNjU3MSIsInB1aWQiOiIxMDAzMDAwMDhDNjM1MDhGIiwic2NwIjoiQ29udGVudC5DcmVhdGUgRGFzaGJvYXJkLlJlYWQuQWxsIERhdGFzZXQuUmVhZFdyaXRlLkFsbCBHcm91cC5SZWFkIFJlcG9ydC5SZWFkV3JpdGUuQWxsIiwic3ViIjoiaGdsbWpTYnNzR0E4ek5SRk00SVpheGVEeHRYTjdHX0R1NHlqOW5LMzU1ZyIsInRpZCI6Ijg2YmE2NzU0LWFkZTEtNDhjZS1hZDNhLTczODg0OTUzNzdiNiIsInVuaXF1ZV9uYW1lIjoiYWRtaW5ASk1GaW5hbmNpYWwub25taWNyb3NvZnQuY29tIiwidXBuIjoiYWRtaW5ASk1GaW5hbmNpYWwub25taWNyb3NvZnQuY29tIiwidmVyIjoiMS4wIiwid2lkcyI6WyI2MmU5MDM5NC02OWY1LTQyMzctOTE5MC0wMTIxNzcxNDVlMTAiXX0.Z6w76eK2OdbS0D5E2z4DxBVOJFXXMydqV811WrNEn-6CNVy4KcXytj3PXKcOKEGeLY1EHqIaGtn8zTzqpePePjhUU47pQs5cDISJRk2vSO1l67hX_mqiJxa4xRGAv3I4edf8sCaLL9uZW4GvLgl45apK3YHx_aPbkHXSmvtQNrIqE7A04hWXh0jgz5od9OpoGyN6NR-FD4FO00EaZTvDh78xPxlSWGY2zlq07gUsY0gddiQ1FsFwP3qGDZmGC5-ioyTVUnl9d0dDJZffGfjF4oLKO7Yrm1NwD4kSeKsnXhJN4BZ7jykPLWsiiNMrV197ApBBpYtXTCbYVNgmRQIKpg"));
            //request.Headers.Add("Authorization", String.Format("Bearer {0}", authResult.AccessToken));

            //Get dashboards response from request.GetResponse()
            using (var response = request.GetResponse() as System.Net.HttpWebResponse)
            {
                //Get reader from response stream
                using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    responseContent = reader.ReadToEnd();

                    //Deserialize JSON string
                    PBIDashboards PBIDashboards = JsonConvert.DeserializeObject<PBIDashboards>(responseContent);

                    if (PBIDashboards != null)
                    {
                        var gridViewDashboards = PBIDashboards.value.Select(dashboard => new {
                            Id = dashboard.id,
                            DisplayName = dashboard.displayName,
                            EmbedUrl = dashboard.embedUrl
                        });

                        this.GridView1.DataSource = gridViewDashboards;
                        this.GridView1.DataBind();
                    }
                }
            }
        }
    }

    public class PBIDashboards
    {
        public PBIDashboard[] value { get; set; }
    }

    public class PBIDashboard
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string embedUrl { get; set; }
        public bool isReadOnly { get; set; }
    }

   
}

public class AccessToken
{
    public string token_type;
    public string scope { get; set; }
    public string expires_in { get; set; }
    public string expires_on { get; set; }
    public string not_before { get; set; }
    public string resource { get; set; }
    public string access_token { get; set; }
    public string refresh_token { get; set; }
    public string id_token { get; set; }
}