﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using PharmacySystem.APIIntergration;
using PharmacySystem.APIIntergration.Utilities;
using PharmacySystem.Models;
using PharmacySystem.Models.Request;
using PharmacySystem.Models.ViewModels;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace PharmacySystem.WebAdmin.Controllers
{
    public class ImportInvoiceController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IInvoiceApiClient _invoiceApiClient;
        private readonly ISupplierApiClient _supplierApiClient;
        private readonly IMedicineApiClient _medicineApiClient;

        public ImportInvoiceController(IConfiguration configuration, IInvoiceApiClient invoiceApiClient, ISupplierApiClient supplierApiClient, IMedicineApiClient medicineApiClient)
        {
            this._configuration = configuration;
            this._invoiceApiClient = invoiceApiClient;
            this._supplierApiClient = supplierApiClient;
            this._medicineApiClient = medicineApiClient;
        }
        public async Task<IActionResult> Index(DateTime? DateCheckIn, DateTime? DateCheckOut, long? IdSupplier, int? StatusID)
        {
            var request = new GetManageIInvoicePagingRequest()
            {
                DateCheckIn = DateCheckIn,
                DateCheckOut = DateCheckOut,
                IdSupplier = IdSupplier,
                StatusID = StatusID
            };
            var data = await _invoiceApiClient.GetImportInvoice(request);
            ViewBag.DateCheckIn = DateCheckIn;
            ViewBag.DateCheckOut = DateCheckOut;

            var supplierList = await _supplierApiClient.GetListSupplier();
            ViewBag.ListOfSupplier = supplierList.Select(x => new SelectListItem()
            {
                Text = x.SupplierName,
                Value = x.IdSupplier.ToString(),
                Selected = IdSupplier.HasValue && IdSupplier.Value == x.IdSupplier
            });
            return View(data);
        }
        public async Task<IActionResult> Create()
        {
            var supplierList = await _supplierApiClient.GetListSupplier();
            ViewBag.ListOfSupplier = supplierList.Select(x => new SelectListItem()
            {
                Text = x.SupplierName,
                Value = x.IdSupplier.ToString(),
            });
            var medicineList = await _medicineApiClient.GetListMedicine();
            ViewBag.ListOfMedicine = medicineList.Select(x => new SelectListItem()
            {
                Text = x.MedicineName,
                Value = x.IdMedicine.ToString(),
            });
            return View(GetIInvoiceViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Create(ImportInvoiceCreateRequest CreateIInvoiceForm)
        {
            var model = GetIInvoiceViewModel();
            var invoiceDetails = new List<InvoiceDetailsVM>();
            foreach (var item in model.IInvoiceItems)
            {
                invoiceDetails.Add(new InvoiceDetailsVM()
                {
                    MedicineId = item.IIdMedicine,
                    MedicineName = item.IMedicineName,
                    Quantity = item.IQuantity,
                    TotalPrice = item.IQuantity * item.IPrice
                });
            }
            var createRequest = new ImportInvoiceCreateRequest()
            {
                IdAccount = 1,
                DateCheckIn = DateTime.Now,
                StatusID = 1,
                Note = CreateIInvoiceForm.Note,
                IdSupplier = CreateIInvoiceForm.IdSupplier,
                InvoiceDetails = invoiceDetails
            };
            await _invoiceApiClient.CreateImportInvoice(createRequest);
            return View(model);
        }
        public async Task<IActionResult> Details(long id)
        {
            var details = await _invoiceApiClient.GetImportInvoiceByID(id);

            var supplierList = await _supplierApiClient.GetListSupplier();
            ViewBag.ListOfSupplier = supplierList.Select(x => new SelectListItem()
            {
                Text = x.SupplierName,
                Value = x.IdSupplier.ToString(),
                Selected = details.IdSupllier.HasValue && details.IdSupllier.Value == x.IdSupplier
            });
            return View(details);
        }
        [HttpGet]
        public IActionResult GetListItems()
        {
            var session = HttpContext.Session.GetString(SystemConstants.IInvoice);
            List<InvoiceDetailsVM> currentCart = new List<InvoiceDetailsVM>();
            if (session != null)
                currentCart = JsonConvert.DeserializeObject<List<InvoiceDetailsVM>>(session);
            return Ok(currentCart);
        }
        public async Task<IActionResult> AddToInvoice(long id, int quantity)
        {
            var medicine = await _medicineApiClient.GetById(id);
            var session = HttpContext.Session.GetString(SystemConstants.IInvoice);
            List<IInvoice> currentInvoice = new List<IInvoice>();
            if (session != null)
                currentInvoice = JsonConvert.DeserializeObject<List<IInvoice>>(session);

            var existItem = currentInvoice.FirstOrDefault(x => x.IIdMedicine == id);
            if(existItem != null)
            {
                quantity = quantity != 0 ? quantity : 1;
                existItem.IQuantity = existItem.IQuantity + quantity;
            } 
            else
            {
                quantity = quantity != 0 ? quantity : 1;
                var invoiceItems = new IInvoice()
                {
                    IIdMedicine = id,
                    IMedicineName = medicine.MedicineName,
                    IQuantity = quantity,
                    IPrice = medicine.ImportPrice
                };

                currentInvoice.Add(invoiceItems);
            }
            HttpContext.Session.SetString(SystemConstants.IInvoice, JsonConvert.SerializeObject(currentInvoice));
            return Ok(currentInvoice);
        }
        public IActionResult UpdateInvoice(int id, int quantity)
        {
            var session = HttpContext.Session.GetString(SystemConstants.IInvoice);
            List<IInvoice> currentInvoice = new List<IInvoice>();
            if (session != null)
                currentInvoice = JsonConvert.DeserializeObject<List<IInvoice>>(session);

            foreach(var item in currentInvoice)
            {
                if(item.IIdMedicine == id)
                { 
                    if(quantity == 0)
                    {
                        currentInvoice.Remove(item);
                        break;
                    }
                    item.IQuantity = quantity;
                }
            }

            HttpContext.Session.SetString(SystemConstants.IInvoice, JsonConvert.SerializeObject(currentInvoice));
            return Ok();
        }
        public IActionResult RemoveItemsInvoice(int id)
        {
            var session = HttpContext.Session.GetString(SystemConstants.IInvoice);
            List<IInvoice> currentInvoice = new List<IInvoice>();
            if (session != null)
                currentInvoice = JsonConvert.DeserializeObject<List<IInvoice>>(session);
            foreach (var item in currentInvoice.ToList())
            {
                if (item.IIdMedicine == id)
                    currentInvoice.Remove(item);
            }

            HttpContext.Session.SetString(SystemConstants.IInvoice, JsonConvert.SerializeObject(currentInvoice));
            return Ok();
        }
        private IInvoiceVM GetIInvoiceViewModel()
        {
            var session = HttpContext.Session.GetString(SystemConstants.IInvoice);
            List<IInvoice> currentInvoice = new List<IInvoice>();
            if(session != null)
                currentInvoice = JsonConvert.DeserializeObject<List<IInvoice>>(session);
            var iinvoiceVM = new IInvoiceVM()
            {
                IInvoiceItems = currentInvoice,
                CreateIInvoiceModel = new ImportInvoiceCreateRequest()
            };
            return iinvoiceVM;
        }
    }
}
