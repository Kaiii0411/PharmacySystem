﻿using Microsoft.AspNetCore.Mvc;
using PharmacySystem.Models;
using PharmacySystem.Models.Request;
using PharmacySystem.Service;

namespace PharmacySystem.WebAPI.Controllers
{
    [Route("api/store")]
    [ApiController]
    public class StoreController : Controller
    {
        private readonly IStoreService _StoreService;
        public StoreController(IStoreService storeService)
        {
            _StoreService = storeService;
        }
        [HttpPost]
        public async Task<RequestResponse> Create(StoreCreateRequest request)
        {
            var StoreId = await _StoreService.Create(request);
            if (StoreId == 0)
            {
                return new RequestResponse
                {
                    StatusCode = Code.Failed,
                    Message = "Add Failed!"
                };
            }
            return new RequestResponse
            {
                StatusCode = Code.Success,
                Message = "Add sucess!"
            };
        }
        [HttpPut]
        public async Task<RequestResponse> Update(StoreUpdateRequest request)
        {
            var StoreId = await _StoreService.Update(request);
            if (StoreId == 0)
            {
                return new RequestResponse
                {
                    StatusCode = Code.Failed,
                    Message = "Update Failed!"
                };
            }
            return new RequestResponse
            {
                StatusCode = Code.Success,
                Message = "Update sucess!"
            };
        }
        [HttpDelete("{storeId}")]
        public async Task<RequestResponse> Delete(long storeId)
        {
            var store = await _StoreService.Delete(storeId);
            if (store == 0)
            {
                return new RequestResponse
                {
                    StatusCode = Code.Failed,
                    Message = $"Delete Failed! Cannot find a store: {storeId}"
                };
            }
            return new RequestResponse
            {
                StatusCode = Code.Success,
                Message = "Delete sucess!"
            };
        }
    }
}
