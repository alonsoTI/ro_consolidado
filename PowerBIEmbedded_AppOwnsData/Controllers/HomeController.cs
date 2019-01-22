using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using PowerBIEmbedded_AppOwnsData.Models;
using PowerBIEmbedded_AppOwnsData.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using PowerBIEmbedded_AppOwnsData.Models;
using Microsoft.SqlServer.Dts.Runtime;
using System.IO;
using System.Web.UI;

namespace PowerBIEmbedded_AppOwnsData.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmbedService m_embedService;
        private static readonly string RutaFS = ConfigurationManager.AppSettings["RutaFS"];
        public System.Web.UI.ClientScriptManager ClientScript { get; }

        public HomeController()
        {
            m_embedService = new EmbedService();
        }

        public ActionResult Index()
        {
            var result = new IndexConfig();
            var assembly = Assembly.GetExecutingAssembly().GetReferencedAssemblies().Where(n => n.Name.Equals("Microsoft.PowerBI.Api")).FirstOrDefault();
            if (assembly != null)
            {
                result.DotNETSDK = assembly.Version.ToString(3);
            }
            return View(result);
        }

        public async Task<ActionResult> EmbedReport(string username, string roles)
        {
            var embedResult = await m_embedService.EmbedReport(username, roles);
            if (embedResult)
            {
                return View(m_embedService.EmbedConfig);
            }
            else
            {
                return View(m_embedService.EmbedConfig);
            }
        }

        public async Task<ActionResult> EmbedDashboard()
        {
            var embedResult = await m_embedService.EmbedDashboard();
            if (embedResult)
            {
                return View(m_embedService.EmbedConfig);
            }
            else
            {
                return View(m_embedService.EmbedConfig);
            }
        }

        public async Task<ActionResult> EmbedTile()
        {
            var embedResult = await m_embedService.EmbedTile();
            if (embedResult)
            {
                return View(m_embedService.TileEmbedConfig);
            }
            else
            {
                return View(m_embedService.TileEmbedConfig);
            }
        }




        [HttpPost]
        public ActionResult Subir(HttpPostedFileBase file, String mes, String año)
        {

            string con = System.Configuration.ConfigurationManager.ConnectionStrings["DBInversionRO"].ConnectionString;
            SqlConnection conexion = null;
            SqlTransaction transaccion = null;

            string archivo = Path.GetFileName( file.FileName);
            String tempPath = RutaFS + archivo;
             file.SaveAs(tempPath);

            //EjecutarETL(tempPath, año + mes + "01");
            conexion = new SqlConnection
                {
                    ConnectionString = con
                };
                conexion.Open();


                transaccion = conexion.BeginTransaction(System.Data.IsolationLevel.Serializable);
                SqlCommand comando = new SqlCommand("ETL_RO", conexion, transaccion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.Clear();
                comando.Parameters.AddWithValue("@ExcelPath", tempPath);
                comando.Parameters.AddWithValue("@periodo", año + mes + "01");
                comando.BeginExecuteNonQuery(new AsyncCallback(AsyncCommandCompletionCallback), comando);
              

            return RedirectToAction("Index");

        }

        private void AsyncCommandCompletionCallback(IAsyncResult result)
        {
            SqlCommand cmd = null;
            try
            {
                cmd = (SqlCommand)result.AsyncState;
                cmd.EndExecuteNonQuery(result);
            }
            catch (Exception ex)
            {
            }
        }

        public void EjecutarETL(String path, String periodo)
        {

            string pkgLocation;
            Package pkg;
            Application app;
            DTSExecResult pkgResults;

            MyEventListener eventListener = new MyEventListener();

            pkgLocation =
              @"\\srvprodbd-ds\ETL\RO_CONSOLIDADO\CargaConcepto.dtsx";

            app = new Application();
            pkg = app.LoadPackage(pkgLocation, eventListener);
            Variables vars = pkg.Variables;

            vars["ExcelPath"].Value = path;

            pkgResults = pkg.Execute(null, null, eventListener, null, null);

            Console.WriteLine(pkgResults.ToString());
            Console.ReadKey();
        }

        class MyEventListener : DefaultEvents
        {
            public override bool OnError(DtsObject source, int errorCode, string subComponent,
              string description, string helpFile, int helpContext, string idofInterfaceWithError)
            {
                // Add application-specific diagnostics here.  
                Console.WriteLine("Error in {0}/{1} : {2}", source, subComponent, description);
                return false;
            }
        }
    }

   

}
