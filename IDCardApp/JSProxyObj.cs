namespace IDCardApp
{
    public delegate JSResult ReceiveHandler(string cmd, string arg);
    public class JSProxyObj
    {
        public JSProxyObj() { }
        public JSProxyObj(ReceiveHandler handler)
        {
            OnReceive = handler;
        }
        public ReceiveHandler OnReceive;
        public JSResult Execute(string cmd, string arg = "")
        {
            return OnReceive(cmd, arg);
        }        
    }

}
