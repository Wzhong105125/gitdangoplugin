using System;
using System.Windows.Forms;

using KeePass.UI;
using KeePass.Plugins;
using KeePassLib.Keys;
using KeePassLib.Utility;
using KeePassLib.Serialization;
using KeyProviderTest.Forms;

namespace KeyProviderTest
{
    public sealed class KeyProviderTestExt : Plugin
    {
        private IPluginHost m_host = null;
        private SampleKeyProvider m_prov = new SampleKeyProvider();

        public override bool Initialize(IPluginHost host)
        {
            m_host = host;

            m_host.KeyProviderPool.Add(m_prov);
            return true;
        }

        public override void Terminate()
        {
            m_host.KeyProviderPool.Remove(m_prov);
        }
    }

    public sealed class SampleKeyProvider : KeyProvider
    {
        private const string AuxFileExt = ".secret.xml";
        private const string ProvType = "empty";
        private const string ProvVersion = "2.0";
        public string key = "1";
        private string Password = String.Empty;
        
        public override string Name
        {
            get { return "DangoPass"; }
        }
        
        
        public override byte[] GetKey(KeyProviderQueryContext ctx)
        {
            try
            {
                
                
                byte[] pb = (ctx.CreatingNewKey ? Create(ctx) : Open(ctx));

                
                if (pb == null) { return null; }
                
                if (pb == new byte[] { 1 }) { return null; }

                // KeePass clears the returned byte array, thus make a copy
                byte[] pbRet = new byte[pb.Length];
                Array.Copy(pb, pbRet, pb.Length);
                
                return pbRet;
            }
            catch (Exception ) {}

            return null;

        }
        private static IOConnectionInfo GetAuxFileIoc(KeyProviderQueryContext ctx)
        {
            return GetAuxFileIoc(ctx.DatabaseIOInfo);
        }
        internal static IOConnectionInfo GetAuxFileIoc(IOConnectionInfo iocBase)
        {
            IOConnectionInfo ioc = iocBase.CloneDeep();
            ioc.Path = UrlUtil.StripExtension(ioc.Path) + AuxFileExt;
            return ioc;
        }
        private static byte[] Create(KeyProviderQueryContext ctx)
        {
            
            IOConnectionInfo iocPrev = GetAuxFileIoc(ctx);
            
            Info Info = Info.Load(iocPrev);
            if (Info == null) Info = new Info();

            CreateFingerPrint cfp = new CreateFingerPrint();
            UIUtil.ShowDialogAndDestroy(cfp);

            if (cfp.Access() != true)
                return null;

            Creation dlge = new Creation();
            
            dlge.InitEx(Info, ctx);
            
            UIUtil.ShowDialogAndDestroy(dlge);
            if (!CreateAuxFile(Info, ctx)) return null;
            
            return Info.Secret;
        }
        private static bool CreateAuxFile(Info Info,
            KeyProviderQueryContext ctx)
        {
            IOConnectionInfo ioc = GetAuxFileIoc(ctx);

            if (!Info.Save(ioc, Info))
            {
                MessageService.ShowWarning("Failed to save auxiliary OTP info file:",
                    ioc.GetDisplayName());
                return false;
            }
            return true;
        }
        public static byte[] Open(KeyProviderQueryContext ctx)
        {

            IOConnectionInfo ioc = GetAuxFileIoc(ctx);

            Info Info = Info.Load(ioc);
            DirectOrDango dd = new DirectOrDango();
            UIUtil.ShowDialogAndDestroy(dd);
            try{
                if (dd.Access() == 0)
                {
                    Form1 dlg = new Form1();
                    dlg.InitEx(Info, ctx);
                    if (Info == null)
                    {

                        return null;
                    }


                    UIUtil.ShowDialogAndDestroy(dlg);

                    if (dlg.Access() != true)
                        return null;




                    if (!CreateAuxFile(Info, ctx)) return null;

                    return Info.Secret;
                }
                else if(dd.Access() == 1)
                {
                    try
                    {
                        
                        Login dlgi = new Login();
                        
                        dlgi.InitEx(Info, ctx);
                        if (Info == null)
                        {

                            return null;
                        }


                        UIUtil.ShowDialogAndDestroy(dlgi);

                        if (System.Text.Encoding.UTF8.GetString(dlgi.Access()) != System.Text.Encoding.UTF8.GetString(Info.Secret))
                            return null;




                        if (!CreateAuxFile(Info, ctx)) return null;

                        return Info.Secret;
                    }
                    catch (Exception ) { }

                    return null;
                }
                else
                {
                    FingerPrint fingerprint = new FingerPrint();
                    
                    UIUtil.ShowDialogAndDestroy(fingerprint);
                    
                    if (fingerprint.Access() != true)
                        return null;
                   
                        

                    DangoLite dlg2 = new DangoLite();      
                    dlg2.InitEx(Info, ctx);
                    if (Info == null)
                    {

                        return null;
                    }
                    UIUtil.ShowDialogAndDestroy(dlg2);

                    if (dlg2.Access() != true)
                        return null;

                    if (!CreateAuxFile(Info, ctx)) return null;

                    return Info.Secret;
                }
            }catch (Exception ex) { MessageService.ShowWarning(ex.Message); }

            return null;

        }
        
        
       
       
     }
}
