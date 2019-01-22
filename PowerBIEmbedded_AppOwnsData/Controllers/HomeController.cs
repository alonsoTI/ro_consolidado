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
using System.IO;

namespace PowerBIEmbedded_AppOwnsData.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmbedService m_embedService;

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

                string archivo =año + mes + file.FileName;
                string con = System.Configuration.ConfigurationManager.ConnectionStrings["DBInversionRO"].ConnectionString;
            SqlConnection conexion = null;
                SqlTransaction transaccion = null;
                String tempPath = "//srvprodbd-ds/ETL/RO_CONSOLIDADO/Archivos/" + archivo;
                file.SaveAs(tempPath);
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
            return RedirectToAction("index");
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
    }

}
