using InventorySystem.Data;
using InventorySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.Text;
namespace InventorySystem.Controllers
{
    public class ChangeLogController : Controller
    {
        private readonly DbInventoryContext _context;
        private readonly IConverter _converter;
        public ChangeLogController(DbInventoryContext context, IConverter converter)
        {
            _context = context;
            _converter = converter;
        }
        public async Task<IActionResult> Index(string searchName, string orderFilter, int? numpag, string currentFilter, string currentOrder, string actionType, string currentActionType)
        {
            ViewData["Is64Bit"] = Environment.Is64BitProcess;
            var logsQuery = _context.ChangeLogs
                .OrderByDescending(log => log.DateMod)
                .AsQueryable();

            // Obtener valores únicos de ActionType
            var actionTypes = await _context.ChangeLogs
                .Where(log => log.TypeAction != null)
                .Select(log => log.TypeAction)
                .Distinct()
                .OrderBy(type => type)
                .ToListAsync();
            // Pasar los valores únicos a la vista
            ViewBag.typeAction = new SelectList(actionTypes);

            // Si hay una nueva búsqueda, reinicia la página a 1
            if (!string.IsNullOrEmpty(searchName))
            {
                numpag = 1;
            }
            else
            {
                searchName = currentFilter; // Usar el filtro actual si no hay nueva búsqueda
            }

            ViewData["CurrentFilter"] = searchName;

            // Aplicar búsqueda por nombre
            if (!string.IsNullOrEmpty(searchName))
            {
                logsQuery = logsQuery.Where(p => p.UserId.Contains(searchName));
            }

            // Aplicar búsqueda por Action TYpe
            if (!string.IsNullOrEmpty(actionType))
            {
                logsQuery = logsQuery.Where(p => p.TypeAction == actionType);
                ViewData["currentActionType"] = actionType; // Guarda el filtro actual
            }
            else
            {
                actionType = currentActionType; // Mantener el orden actual si no se proporciona uno nuevo
                ViewData["currentActionType"] = currentActionType; // Asegúrate de que se pase a la vista
            }


            // Orden dinámico
            if (!string.IsNullOrEmpty(orderFilter))
            {
                logsQuery = orderFilter switch
                {
                    "asc" => logsQuery.OrderBy(p => p.DateMod),
                    "desc" => logsQuery.OrderByDescending(p => p.DateMod),
                    _ => logsQuery
                };
            }
            else
            {
                orderFilter = currentOrder; // Mantener el orden actual si no se proporciona uno nuevo
            }

            ViewData["CurrentOrder"] = orderFilter;

            // Lista de opciones para el orden
            ViewBag.orderFilter = new SelectList(new[]
            {
        new { Text = "Ascendent Order", Value = "asc" },
        new { Text = "Descendent Order", Value = "desc" }
    }, "Value", "Text", orderFilter);

            int regQuantity = 5;
            return View(await Pagination<ChangeLog>.CreatePagination(logsQuery.AsNoTracking(), numpag ?? 1, regQuantity));
        }

        public IActionResult GeneratePdf()
        {
            // Obtener todos los datos de la tabla ChangeLog
            var logs = _context.ChangeLogs.AsNoTracking().ToList();

            // Construir el contenido HTML para el PDF usando StringBuilder para mejorar el rendimiento
            var htmlContent = new StringBuilder();
            htmlContent.Append(@"
        <html>
        <head>
            <style>
                body {
                    font-family: Arial, sans-serif;
                    margin: 0;
                    padding: 0;
                }
                h1 {
                    text-align: center;
                    font-size: 24px;
                    margin-bottom: 20px;
                }
                table {
                    width: 100%;
                    border-collapse: collapse;
                    table-layout: fixed;
                }
                th, td {
                    border: 1px solid #ddd;
                    padding: 8px;
                    text-align: left;
                    word-wrap: break-word; /* Evita desbordamientos en las celdas */
                }
                th {
                    background-color: #f2f2f2;
                    font-weight: bold;
                }
                tr {
                    page-break-inside: avoid; /* Evita que una fila se corte entre páginas */
                }
            </style>
        </head>
        <body>
            <h1>Change Log Report</h1>
            <table>
                <thead>
                    <tr>
                        <th>Id</th>
                        <th>UserMail</th>
                        <th>ActionType</th>
                        <th>TableName</th>
                        <th>DateMod</th>
                        <th>OldValues</th>
                        <th>NewValues</th>
                    </tr>
                </thead>
                <tbody>");

            foreach (var log in logs)
            {
                htmlContent.Append("<tr>")
                           .AppendFormat("<td>{0}</td>", log.Id)
                           .AppendFormat("<td>{0}</td>", log.UserId)
                           .AppendFormat("<td>{0}</td>", log.TypeAction)
                           .AppendFormat("<td>{0}</td>", log.TableName)
                           .AppendFormat("<td>{0:yyyy-MM-dd HH:mm:ss}</td>", log.DateMod)
                           .AppendFormat("<td>{0}</td>", log.OldValues)
                           .AppendFormat("<td>{0}</td>", log.NewValues)
                           .Append("</tr>");
            }

            htmlContent.Append(@"
                </tbody>
            </table>
        </body>
        </html>");

            // Configurar el documento PDF
            var pdfDoc = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Landscape,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                }
            };

            pdfDoc.Objects.Add(new ObjectSettings
            {
                HtmlContent = htmlContent.ToString(),
                WebSettings = { DefaultEncoding = "utf-8" }
            });

            // Convertir a PDF
            var pdf = _converter.Convert(pdfDoc);

            // Retornar el PDF como archivo descargable
            return File(pdf, "application/pdf", "ChangeLog.pdf");
        }


    }
}
