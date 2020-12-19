using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitTestMVC.Controllers;
using UnitTestMVC.Models;
using UnitTestMVC.Repository;
using Xunit;

namespace UnitTestMVC.Test
{
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;

        private readonly ProductsController _controller;

        private List<Product> products;

        public ProductControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();

            _controller = new ProductsController(_mockRepo.Object);

            products = new List<Product>()
            {
                new Product(){Id = 2, Name="Kalem" ,Stock = 40 , Color="Kırmızı" , Price = 100},
                new Product(){Id = 3, Name="Defter" ,Stock = 40 , Color="Sarı" , Price = 200},
            };
        }

        [Fact]
        public async void Index_ActionExecute_ReturnView()
        {
            var result = await _controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void Index_ActionExecute_ReturnProductList()
        {
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(products);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);

            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);

            Assert.Equal<int>(2, productList.Count());
        }

        [Fact]
        public async void Details_IdIsNull_ReturnRedirectToIndex()
        {
            var result = await _controller.Details(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void Details_IdInvalid_ReturnNotFound()
        {
            Product product = null;

            _mockRepo.Setup(x => x.GetById(0)).ReturnsAsync(product);

            var result = await _controller.Details(0);

            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404, redirect.StatusCode);

        }

        [Theory]
        [InlineData(2)]
        public async void Details_ValidId_ReturnProduct(int productId)
        {
            Product product = products.First(x => x.Id == productId);

            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Details(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Fact]
        public void Create_ActionExecute_ReturnView()
        {
            var result = _controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void CreatePOST_InvalidModelState_ReturnView()
        {
            _controller.ModelState.AddModelError("Name", "Name Gereklidir");

            var result = await _controller.Create(products[0]);

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsType<Product>(viewResult.Model);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_ReturnToIndex()
        {
            var result = await _controller.Create(products.First());

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void CreatePOST_ValidModelState_CreateMethodExecute()
        {
            Product newProduct = null;

            _mockRepo.Setup(repo => repo.Create(It.IsAny<Product>())).Callback<Product>(x => newProduct = x);

            var result = await _controller.Create(products.First());

            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Once);

            Assert.Equal(products.First().Id, newProduct.Id);
        }

        [Fact]
        public async void CreatePOS_InValidModelState_NeverCreateExecute()
        {
            _controller.ModelState.AddModelError("Name", "Name Alanı Gereklidir");

            var result = await _controller.Create(products.First());

            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async void Edit_IdIsNull_ReturnToIndex()
        {
            var result = await _controller.Edit(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void Edit_IdInvalid_ReturnNotFound()
        {
            Product product = null;

            _mockRepo.Setup(repo => repo.GetById(0)).ReturnsAsync(product);

            var result = await _controller.Edit(0);

            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(2)]
        public async void Edit_IdValid_ReturnView(int productId)
        {
            Product product = products.First(x => x.Id == productId);

            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Edit(productId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var resultModel = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(product.Id, resultModel.Id);
        }

        [Theory]
        [InlineData(2)]
        public void EditPOST_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            var product = products.First(x => x.Id == productId);
            var result = _controller.Edit(3, product);

            var redirect = Assert.IsType<NotFoundResult>(result);

        }

        [Theory]
        [InlineData(2)]
        public void EditPOST_ProductNotValid_ReturnView(int productId)
        {
            var product = products.First(x => x.Id == productId);

            _controller.ModelState.AddModelError("Name", "");
            var result = _controller.Edit(productId, product);

            var redirect = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(redirect.Model);
        }

        [Theory]
        [InlineData(2)]
        public void EditPOST_ProductValid_ReturnToAction(int productId)
        {
            var product = products.First(x => x.Id == productId);
            var result = _controller.Edit(productId, product);
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(2)]
        public void EditPOST_ProductValid_EditMethodExecute(int productId)
        {

            var product = products.First(x => x.Id == productId);
            product.Name = "Test";

            _mockRepo.Setup(repo => repo.Update(product));

            var result = _controller.Edit(productId, product);


            _mockRepo.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async void Delete_IdNotValid_ReturnNotFound()
        {
            var result = await _controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void Delete_ProductNotFound_ReturnNotFound()
        {
            Product product = null;

            _mockRepo.Setup(repo => repo.GetById(0)).ReturnsAsync(product);

            var result = await _controller.Edit(0);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(2)]
        public async void Delete_IdValid_ReturnView(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);
            var redirect = Assert.IsType<ViewResult>(result);

            Assert.IsAssignableFrom<Product>(redirect.Model);
        }

        [Theory]
        [InlineData(2)]
        public async void DeleteConfirm_ActionExecute_ReturnRedirectToAction(int productId)
        {
            var result = await _controller.DeleteConfirmed(productId);

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Theory]
        [InlineData(2)]
        public async void DeleteConfirm_ActionExecute_DeleteMethodExecute(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            _mockRepo.Setup(x => x.Delete(product));

            var result = await _controller.DeleteConfirmed(productId);

            _mockRepo.Verify(Repository => Repository.Delete(It.IsAny<Product>()), Times.Once);
        }
    }
}
