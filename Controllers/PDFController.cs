using IdeaPDFViewer.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace IdeaPDFViewer.Controllers
{
    public class PDFController : Controller
    {
        // GET: PDF

        public async Task<ActionResult> Viewer(string guidString)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(guidString))
                {
                    if (Guid.TryParse(guidString, out Guid pdfGuid))
                    {
                        T_PDF findPDF;
                        using (Entity db = new Entity())
                        {
                            findPDF = await db.T_PDF.FirstOrDefaultAsync(x => x.UNIQUE_ID == pdfGuid);
                        }
                        if (findPDF != null)
                        {
                            byte[] pdfBytes = System.IO.File.ReadAllBytes(findPDF.PATH);
                            Response.Headers.Add("Content-Disposition", "inline; filename=" + findPDF.YEAR + "-" + findPDF.MONTH + "-" + findPDF.TC + "-" + findPDF.PAGE + ".pdf");
                            await DownloadLog(findPDF.ID, findPDF.PATH);
                            return File(pdfBytes, "application/pdf");
                        }
                        else
                        {
                            await ErrorLog("Dosya bulunamadı.", null, null, findPDF.PATH);
                            return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Dosya bulunamadı.");
                        }
                    }
                    else
                    {
                        await ErrorLog("Geçersiz GUID formatı.", null, null, null);
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Gecersiz GUID formati.");
                    }
                }
                else
                {
                    await ErrorLog("PDF GUID okunamadı.", null, null, null);
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "PDF GUID okunamadi.");
                }
            }
            catch (Exception ex)
            {
                var inex = ex.InnerException != null ? ex.InnerException.Message.ToString() : null;
                await ErrorLog("Hata, sistem yöneticisiyle iletişime geçin.", ex.Message.ToString(), inex, null);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Hata, sistem yoneticisyle iletisime gecin.");
            }
        }

        public static async Task<string> GetExternalIPAddress()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = "https://api64.ipify.org?format=json"; 
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();

                        dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                        string publicIpAddress = jsonResponse.ip;
                        return publicIpAddress;
                    }
                    else
                    {
                        await ErrorLog("Dış IP Adresi bulunamadı.", null, null, null);
                        throw new InvalidOperationException("Dış IP Adresi bulunamadı.");
                    }
                }
             
            }
            catch (Exception ex)
            {
                var inex = ex.InnerException != null ? ex.InnerException.Message.ToString() : null;
                await ErrorLog("Dış IP Adresi alınırken bir hata oluştu.", ex.Message.ToString(), inex, null);
                throw new InvalidOperationException("Dış IP Adresi alınırken bir hata oluştu: " + ex.Message);
            }
        }

        private static async Task<string> GetInternalIPAddress()
        {
            try
            {
                string internalIp = System.Web.HttpContext.Current.Request.UserHostAddress;
                if (!string.IsNullOrWhiteSpace(internalIp))
                {
                    return internalIp;
                }
                else
                {
                    await ErrorLog("İç IP Adresi bulunamadı.", null, null, null);

                    throw new InvalidOperationException("İç IP Adresi bulunamadı.");
                }

                
            }
            catch (Exception ex)
            {
                var inex = ex.InnerException != null ? ex.InnerException.Message.ToString() : null;

                await ErrorLog("İç IP Adresi alınırken bir hata oluştu.", ex.Message.ToString(), inex, null);
                throw new InvalidOperationException("İç IP Adresi alınırken bir hata oluştu: " + ex.Message);
            }
        }

        private static async Task ErrorLog(string hatanot, string exmsg, string exinner, string konum)
        {
            string dis_ip = await GetExternalIPAddress();
            string ic_ip = await GetInternalIPAddress();
            using (Entity db = new Entity())
            {
                E_LOG errLog = new E_LOG()
                {
                    EX_IP = dis_ip,
                    IN_IP = ic_ip,
                    DATE = DateTime.Now,
                    ERROR_NOTE = hatanot,
                    MESSAGE = exmsg,
                    INNER_EXCEPTION = exinner,
                    PATH = konum
                };
                db.E_LOG.Add(errLog);
                await db.SaveChangesAsync();
             }
        }

        private async Task DownloadLog(int ID, string KONUM)
        {
            string dis_ip = await GetExternalIPAddress();
            string ic_ip = await GetInternalIPAddress();
            using (Entity db = new Entity())
            {
                D_LOG pdfLog = new D_LOG()
                {
                    ID = ID,
                    PATH = KONUM,
                    EX_IP = dis_ip,
                    IN_IP = ic_ip,
                    DATE = DateTime.Now,
                };
                db.D_LOG.Add(pdfLog);
                db.SaveChanges();
            }
        }
    }
}