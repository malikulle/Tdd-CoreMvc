using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnitTestMVC.Controllers;
using UnitTestMVC.Models;
using UnitTestMVC.Repository;
using Xunit;

namespace UnitTestMVC.Test
{
    public class ProductApiControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsApiController _controller;

        private List<Product> products;

        public ProductApiControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsApiController(_mockRepo.Object);
            products = new List<Product>()
            {
                new Product(){Id = 2, Name="Kalem" ,Stock = 40 , Color="Kırmızı" , Price = 100},
                new Product(){Id = 3, Name="Defter" ,Stock = 40 , Color="Sarı" , Price = 200},
            };
        }

        [Fact]
        public async void GetProduct_ActionExecutes_ReturnOkResuktWithProduct()
        {
            _mockRepo.Setup(x => x.GetAll()).ReturnsAsync(products);

            var result = await _controller.GetProduct();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnProdcuts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            Assert.Equal<int>(2, returnProdcuts.ToList().Count);
        }
        [Theory]
        [InlineData(0)]
        public async void GetProduct_IdInvalid_ReturnNotFound(int productId)
        {
            Product product = null;

            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(2)]
        public async void GetProduct_IdValid_ReturnOk(int productId)
        {
            var product = products.First(x => x.Id == productId);

            _mockRepo.Setup((x => x.GetById(productId))).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            var oKResult = Assert.IsType<OkObjectResult>(result);

            var returnProduct = Assert.IsType<Product>(oKResult.Value);

            Assert.Equal(productId,returnProduct.Id);
        }
        [Theory]
        [InlineData(2)]
        public void PutProduct_IdIsNotEqualProduct_ReturnBadRequest(int productId)
        {
            var product = products.First(x => x.Id == productId);

            var result = _controller.PutProduct(3, product);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(2)]
        public void PutProduct_ActionExecutes_ReturnNoContent(int productId)
        {
            var product = products.First(x => x.Id == productId);

            _mockRepo.Setup(x => x.Update(product));

            var result = _controller.PutProduct(productId, product);

            _mockRepo.Verify(x => x.Update(product),Times.Once);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostProduct_ActionExecutes_ReturnCreatedAtAction()
        {
            var product = products.First();

            _mockRepo.Setup(x => x.Create(product)).Returns(Task.CompletedTask);

            var result = await _controller.PostProduct(product);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Theory]
        [InlineData(0)]
        public async void DeleteProduct_IdIsInvalid_ReturnNotFound(int productId)
        {
            Product product = null;

            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.DeleteProduct(productId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Theory]
        [InlineData(2)]
        public async void DeleteProduct_ActionExecute_ReturnNoContent(int productId)
        {

            var product = products.First(x => x.Id == productId);

            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);


            _mockRepo.Setup(x => x.Delete(product));

            var result = await _controller.DeleteProduct(productId);

            _mockRepo.Verify(x => x.Delete(product),Times.Once);

            Assert.IsType<NoContentResult>(result.Result);
        }
    }
}
