using FeatureFlags.Constants;
using FeatureFlags.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace FeatureFlags.Web.Tests.Extensions;

public class ControllerExtensionsTests {
    private readonly Mock<HttpRequest> _MockHttpRequest = new();

    private const string _RequestedWithHeader = "X-Requested-With";
    private const string _XmlHttpRequest = "XMLHttpRequest";

    [Fact]
    public void IsAjaxRequest_WithHeader_ReturnsTrue() {
        // arrange
        _MockHttpRequest.Setup(x => x.Headers[_RequestedWithHeader]).Returns(_XmlHttpRequest);

        // act
        var result = _MockHttpRequest.Object.IsAjaxRequest();

        // assert
        Assert.True(result);
    }

    [Fact]
    public void IsAjaxRequest_WithBadHeader_ReturnsFalse() {
        // arrange
        _MockHttpRequest.Setup(x => x.Headers[_RequestedWithHeader]).Returns("gibberish");

        // act
        var result = _MockHttpRequest.Object.IsAjaxRequest();

        // assert
        Assert.False(result);
    }

    [Fact]
    public void IsAjaxRequest_WithoutHeader_ReturnsFalse() {
        // arrange
        _MockHttpRequest.Setup(x => x.Headers["gibberish"]).Returns("gibberish");

        // act
        var result = _MockHttpRequest.Object.IsAjaxRequest();

        // assert
        Assert.False(result);
    }

    [Fact]
    public void ToSelectList_WithList_ReturnsSelectList() {
        // arrange
        var list = new List<string> { "test1", "test2" };

        // act
        var result = list.ToSelectList(x => $"{x}+", x => $"{x}-");

        // assert
        Assert.IsType<List<SelectListItem>>(result, exactMatch: false);
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            x => Assert.Equal("test1+", x.Text),
            x => Assert.Equal("test2+", x.Text)
        );
        Assert.Collection(result,
            x => Assert.Equal("test1-", x.Value),
            x => Assert.Equal("test2-", x.Value)
        );
    }

    [Fact]
    public void ToSelectList_WithEmptyList_ReturnsEmptySelectList() {
        // arrange
        var list = new List<string>();

        // act
        var result = list.ToSelectList(x => $"{x}+", x => $"{x}-");

        // assert
        Assert.IsType<List<SelectListItem>>(result, exactMatch: false);
        Assert.Empty(result);
    }

    [Fact]
    public void AddError_WithOneModelError_AddsExpectedError() {
        // arrange
        var error = "Test error.";
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("general", error);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

        // act
        viewData.AddError(modelState);

        // assert
        Assert.NotNull(viewData[ViewProperties.Error]);
        Assert.Equal(error, viewData[ViewProperties.Error]);
    }

    [Fact]
    public void AddError_WithEmptyModelError_DoesntAddError() {
        // arrange
        var error = "";
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("general", error);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

        // act
        viewData.AddError(modelState);

        // assert
        Assert.False(viewData.ContainsKey(ViewProperties.Error));
    }

    [Fact]
    public void AddError_WithMultipleModelErrors_AddsExpectedError() {
        // arrange
        var error1 = "Test error.";
        var error2 = "Another test error.";
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("general", error1);
        modelState.AddModelError("general", error2);
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

        // act
        viewData.AddError(modelState);

        // assert
        Assert.NotNull(viewData[ViewProperties.Error]);
        Assert.Equal($"{error1} <br />{error2}", viewData[ViewProperties.Error]);
    }

    [Fact]
    public void AddError_WithNoModelErrors_DoesntAddError() {
        // arrange
        var modelState = new ModelStateDictionary();
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

        // act
        viewData.AddError(modelState);

        // assert
        Assert.False(viewData.ContainsKey(ViewProperties.Error));
    }

    [Fact]
    public void AddError_WithNullModelState_ThrowsError() {
        // arrange
        var modelState = new ModelStateDictionary();
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

        // act
#pragma warning disable CS8604 // Possible null reference argument.
        Assert.Throws<ArgumentNullException>(() => viewData.AddError(null as ModelStateDictionary));
#pragma warning restore CS8604 // Possible null reference argument.
    }

    [Fact]
    public void AddError_WithStringError_AddsExpectedError() {
        // arrange
        var error = "test";
        var modelState = new ModelStateDictionary();
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

        // act
        viewData.AddError(error);

        // assert
        Assert.Equal(error, viewData[ViewProperties.Error]);
    }

    [Fact]
    public void AddError_WithEmptyString_DoesntAddError() {
        // arrange
        var error = "";
        var modelState = new ModelStateDictionary();
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

        // act
        viewData.AddError(error);

        // assert
        Assert.False(viewData.ContainsKey(ViewProperties.Error));
    }

    [Fact]
    public void AddError_WithNull_DoesntAddError() {
        // arrange
        var modelState = new ModelStateDictionary();
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

        // act
#pragma warning disable CS8604 // Possible null reference argument.
        viewData.AddError(null as string);
#pragma warning restore CS8604 // Possible null reference argument.

        // assert
        Assert.False(viewData.ContainsKey(ViewProperties.Error));
    }

    [Fact]
    public void AddMessage_WithStringMessage_AddsExpectedMessage() {
        // arrange
        var message = "test";
        var modelState = new ModelStateDictionary();
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

        // act
        viewData.AddMessage(message);

        // assert
        Assert.Equal(message, viewData[ViewProperties.Message]);
    }

    [Fact]
    public void AddMessage_WithEmptyString_DoesntAddMessage() {
        // arrange
        var message = "";
        var modelState = new ModelStateDictionary();
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

        // act
        viewData.AddMessage(message);

        // assert
        Assert.False(viewData.ContainsKey(ViewProperties.Message));
    }

    [Fact]
    public void AddMessage_WithNull_DoesntAddMessage() {
        // arrange
        var modelState = new ModelStateDictionary();
        var modelMetadataProvider = new EmptyModelMetadataProvider();
        var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

        // act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        viewData.AddMessage(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // assert
        Assert.False(viewData.ContainsKey(ViewProperties.Message));
    }
}
