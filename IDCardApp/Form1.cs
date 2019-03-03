
using CefSharp;
using CefSharp.Internals;
using CefSharp.WinForms;
using IDCardApp.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utils;

namespace IDCardApp
{
    public partial class Form1 : Form
    {
        private ChromiumWebBrowser _webBrowser = null;

        private AutoReadMode _readMode = AutoReadMode.None;
        private System.Timers.Timer _autoReadCardTimer = null;

        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(1280, 660);
            this.CenterToScreen();

            CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            
            var settings = new CefSettings();
            settings.CefCommandLineArgs.Add("disable-gpu", "1"); //解决部分电脑界面偏移现象
            
            Cef.Initialize(settings);

            _webBrowser = new ChromiumWebBrowser(Application.StartupPath + @"\Pages\main.html")
            {
                Dock = DockStyle.Fill,
                KeyboardHandler = new CEFKeyBoardHander()
            };
            
            this.Controls.Add(_webBrowser);

            _autoReadCardTimer = new System.Timers.Timer(1500);
            _autoReadCardTimer.Elapsed += (s, e) => AutoReadCard();
            _autoReadCardTimer.Start();
            
            _webBrowser.RegisterAsyncJsObject("$formProxy", new JSProxyObj(ReceiveCmd));

            _webBrowser.TitleChanged += (s, e) => this.Text = e.Title;
        }

        

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _autoReadCardTimer.Stop();
            _webBrowser.Dispose();
            base.OnFormClosed(e);
        }

        private JSResult ReceiveCmd(string cmd, string arg = "")
        {
            var result = new JSResult() { success = true };
            
            if(cmd.StartsWith("repo."))
            {
                result.jsonData = CmdRouter.RepoRoute(cmd, arg);
            }
            else if(cmd == "showDevTools")
            {
                _webBrowser.ShowDevTools();
            }
            else if(cmd == "read-identity-card")
            {
                var cardInfo = ReadIdentityCard();
                if(cardInfo != null)
                {
                    result.jsonData = JsonUtil.Stringify(cardInfo);
                }
            }
            else if(cmd == "auto-readcard")
            {
                var readMode = AutoReadMode.None;
                Enum.TryParse<AutoReadMode>(arg, out readMode);
                this._readMode = readMode;
            }
            else if(cmd == "submit-auth")
            {
                var dto = JsonUtil.Parse<UserCardAuthDto>(arg);
                var r = SubmitAuth(dto);
                result.success = r == 1;
            }
            else if(cmd == "query-auth")
            {
                var ps = arg.Split(',');
                var dto = new UserCardAuth().GetByCardIdOrUserId(ps[0], ps[1]);
                if(dto != null && dto.User != null)
                {
                    dto.User.SetIdentityPhotoWebPath();
                }
                result.jsonData = JsonUtil.Stringify(dto);
            }
            return result;   
        }

        private void TriggerClient(string key, object data)
        {
            var js = string.Format("$formProxy.trigger('{0}',{1})", key, JsonUtil.Stringify(data));
            _webBrowser.ExecuteScriptAsync(js);
        }

        private void AutoReadCard()
        {
            if(_readMode == AutoReadMode.IdentityCard)
            {
                var card = ReadIdentityCard();
                if (card != null)
                {
                    TriggerClient("read-identitycard", card);
                }
            }
            else if(_readMode == AutoReadMode.ICCardNO)
            {
                var reader = Reader.Singleton;
                if (reader.Connect())
                {
                    var cardNo = reader.ReadICCardNO();
                    if (cardNo.Length > 0)
                    {
                        var authDto = new UserCardAuth().GetByCardNo(cardNo);
                        if(authDto.User != null)
                        {
                            authDto.User.SetIdentityPhotoWebPath();
                        }
                        TriggerClient("read-iccardno", authDto);
                    }
                }
            }
        }

        private Entity.UserInfo ReadIdentityCard()
        {
            Entity.UserInfo userInfo = null;
            var reader = Reader.Singleton;
            
            if(reader.Connect())
            {
                userInfo = reader.ReadIdentityCard();
                reader.DisConnect();
            }
            if(userInfo != null)
            {
                var photo = Path.Combine(Application.StartupPath, "photo.bmp");
                var target = Path.Combine(Application.StartupPath, "Data/photos/" + userInfo.IdentityNumber + ".bmp");
                File.Copy(photo, target, true);
                userInfo.SetIdentityPhotoWebPath();
            }
            return userInfo;
        }

        private int SubmitAuth(UserCardAuthDto dto)
        {
            //写卡
            var writeResult = 0;
            var reader = Reader.Singleton;
            if(reader.Connect())
            {
                //第三扇区
                writeResult = reader.WriteCard(2, dto.GetAuthStr());
                reader.DisConnect();                
            }
            if (writeResult == 1)
            {
                new UserCardAuth().Auth(dto);
            }
            return writeResult;
        }
         

    }

    public enum AutoReadMode
    {
        None,
        IdentityCard,
        ICCardNO
    }
}
