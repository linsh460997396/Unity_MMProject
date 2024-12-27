using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace MetalMaxSystem
{
    public class ImageUploader
    {
        private readonly HttpClient _httpClient;
        private readonly string _uploadUrl; //Web服务的上传URL

        public ImageUploader(string uploadUrl)
        {
            _httpClient = new HttpClient();
            _uploadUrl = uploadUrl; //例如："http://yourserver.com/api/upload"
        }

        public async Task<string> UploadImageAsync(string barcode)
        {
            try
            {
                var filePath = barcode + ".jpg";
                var fileContent = new StreamContent(new FileStream(filePath, FileMode.Open, FileAccess.Read));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

                var formData = new MultipartFormDataContent();
                formData.Add(fileContent, "images", Path.GetFileName(filePath)); //假设服务器端接收的字段名是"images"
                                                                                 //添加其他表单字段，如果有的话
                                                                                 //formData.Add(new StringContent("adult"), "imcontent");
                                                                                 //formData.Add(new StringContent("private"), "privacy");
                                                                                 //formData.Add(new StringContent("3"), "thumb_size");

                var response = await _httpClient.PostAsync(_uploadUrl, formData);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    //这里假设服务器返回的JSON中包含上传图片的链接，需要根据实际返回的结构来解析它
                    //例如：{"link": "http://yourserver.com/images/uploadedimage.jpg"}
                    var link = ParseLinkFromJson(content); //需要实现这个方法来解析链接
                    return link;
                }
                else
                {
                    throw new HttpRequestException("Image upload failed with status code: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading image: " + ex.Message);
                throw;
            }
            finally
            {
                File.Delete(barcode + ".jpg"); //删除本地文件
            }
        }

        private string ParseLinkFromJson(string jsonContent)
        {
            //这里需要根据实际的JSON结构来解析链接
            //假设JSON结构是 {"link": "http://..."}
            //使用Newtonsoft.Json或System.Text.Json来解析JSON
            //这里仅作为示例，需要根据实际情况来实现这个方法
            throw new NotImplementedException("ParseLinkFromJson method needs to be implemented.");
        }

        //使用上述类时，需要提供正确的上传URL，并调用UploadImageAsync方法。这个方法会异步地上传文件，并返回上传后的链接（假设服务器返回了这样的信息）
        //注意，这个示例假设服务器返回的是一个JSON响应，并且需要实现ParseLinkFromJson方法来解析这个响应并获取链接
        //请确保C#项目引用了必要的NuGet包来处理HTTP请求和JSON解析（例如System.Net.Http和Newtonsoft.Json或System.Text.Json）
        //此外，根据实际Web服务API，可能需要调整表单字段的名称和其他请求细节
    }
}