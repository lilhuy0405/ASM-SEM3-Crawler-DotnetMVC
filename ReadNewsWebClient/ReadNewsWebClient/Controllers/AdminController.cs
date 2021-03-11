﻿using Newtonsoft.Json;
using PagedList;
using ReadNewsWebClient.API;
using ReadNewsWebClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ReadNewsWebClient.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult CreateConfig()
        {

            var listCategory = GetCategory();
            ViewBag.ListCategory = listCategory;
            return View();
        }

        [HttpPost]
        public ActionResult CreateConfig([Bind(Include = "Path,Route,LinkSelector,ContentSelector,TitleSelector,DescriptionSelector,RemovalSelector,CategoryId")] CrawlerConfigsViewModel crawlerConfigsViewModel)
        {

            var createConfig = ApiEndPoint.ApiDomain + ApiEndPoint.CreateConfigPath;
            try
            {

                using (HttpClient httpClient = new HttpClient())
                {

                    var jsonString = JsonConvert.SerializeObject(crawlerConfigsViewModel);
                    var data = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    HttpResponseMessage getListResult = httpClient.PostAsync(createConfig, data).Result;
                    if (!getListResult.IsSuccessStatusCode)
                    {

                        //request failed
                        Debug.WriteLine("failed");

                        return View(crawlerConfigsViewModel);

                    }
                    Debug.WriteLine("OK");
                    var jsonResult = getListResult.Content.ReadAsStringAsync().Result;
                    var newConfig = JsonConvert.DeserializeObject<CrawlerConfigsViewModel>(jsonResult);
                    return RedirectToAction("ListResource");
                }
            }
            catch (Exception err)
            {

                Debug.WriteLine(err.Message);
                return View(crawlerConfigsViewModel);
            }

        }

        public ActionResult PopUpPreviewUrl()
        {
            return PartialView();
        }

        public ActionResult ListResource()
        {
            var list = new List<CrawlerConfigsViewModel>();
            var getListResourceUrl = ApiEndPoint.ApiDomain + ApiEndPoint.GetListCrawlerConfigPath;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {


                    HttpResponseMessage getListResult = httpClient.GetAsync(getListResourceUrl).Result;
                    if (!getListResult.IsSuccessStatusCode)
                    {

                        //request failed
                        TempData["GetListStatus"] = "Run url crawler Failed!";
                        return View(list);
                    }

                    var jsonResult = getListResult.Content.ReadAsStringAsync().Result;
                    var listConfigs = JsonConvert.DeserializeObject<List<CrawlerConfigsViewModel>>(jsonResult);
                    list = listConfigs;
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                TempData["GetListStatus"] = "Can not connect to API";

            }
            return View(list);
        }

        public ActionResult RunUrlCrawler()
        {
            var runUlrApiUrl = ApiEndPoint.ApiDomain + ApiEndPoint.RunUrlCrawlerPath;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {


                    HttpResponseMessage runResult = httpClient.PostAsync(runUlrApiUrl, null).Result;
                    if (!runResult.IsSuccessStatusCode)
                    {

                        //request failed
                        TempData["RunCrawlerStatus"] = "Run url crawler Failed!";
                        return RedirectToAction("ListResource");
                    }
                    else
                    {
                        TempData["RunCrawlerStatus"] = "Run url crawler Sucess!";
                        return RedirectToAction("ListResource");
                    }
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                TempData["RunCrawlerStatus"] = "Can not connect to API";
                return RedirectToAction("ListResource");
            }
        }

        public ActionResult RunContentCrawler()
        {
            var runContentCrawlerApiUrl = ApiEndPoint.ApiDomain + ApiEndPoint.RunContentCrawlerPath;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {


                    HttpResponseMessage runResult = httpClient.PostAsync(runContentCrawlerApiUrl, null).Result;
                    if (!runResult.IsSuccessStatusCode)
                    {

                        //request failed
                        TempData["RunCrawlerStatus"] = "Run content crawler Failed!";
                        return RedirectToAction("ListResource");
                    }
                    else
                    {
                        TempData["RunCrawlerStatus"] = "Run content crawler Sucess!";
                        return RedirectToAction("ListResource");
                    }
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                TempData["RunCrawlerStatus"] = "Can not connect to API";
                return RedirectToAction("ListResource");
            }

        }

        public ActionResult ListCategory()
        {

            var list = GetCategory();

            if (list.Count() == 0)
            {
                TempData["Status"] = "Can not connect to API";
                return View();
            }

            return View(list);

        }

        public ActionResult ListPendingArticle()
        {
            var listPendingAricle = new List<Article>();
            var getListPendingArticle = ApiEndPoint.ApiDomain + ApiEndPoint.GetListPendingArticlePath;


            try
            {
                using (HttpClient httpClient = new HttpClient())
                {


                    HttpResponseMessage runResult = httpClient.GetAsync(getListPendingArticle).Result;
                    if (!runResult.IsSuccessStatusCode)
                    {

                        //request failed
                        TempData["GetListPendingArticleStatus"] = "Get list pending article Failed!";
                        return View(listPendingAricle);
                    }
                    else
                    {
                        var jsonString = runResult.Content.ReadAsStringAsync().Result;
                        listPendingAricle = JsonConvert.DeserializeObject<List<Article>>(jsonString);
                        var orderByCreatedAt = from article in listPendingAricle orderby article.CreatedAt descending select article;
                        listPendingAricle = orderByCreatedAt.ToList();


                        return View(listPendingAricle);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                TempData["GetListPendingArticleStatus"] = "Can not connect to API";
                return View(listPendingAricle);
            }

        }

        public ActionResult ArticeDetail(int id)
        {
            //call api
            var url = ApiEndPoint.GenerateGetArticleByIdUrl(id);
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {


                    HttpResponseMessage runResult = httpClient.GetAsync(url).Result;
                    if (!runResult.IsSuccessStatusCode)
                    {

                        //request failed
                        TempData["AritcleDetailStatus"] = "Get article detais infor failed at index: " + id;
                        return RedirectToAction("ListPendingArticle");
                    }
                    else
                    {

                        var jsonString = runResult.Content.ReadAsStringAsync().Result;
                        var article = JsonConvert.DeserializeObject<Article>(jsonString);
                        return View(article);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                TempData["AritcleDetailStatus"] = $"{err.Message} at index: {id}";
                return RedirectToAction("ListPendingArticle");
            }

        }

        public ActionResult ListAllArticle()
        {
            var getListAllArticle = ApiEndPoint.ApiDomain + ApiEndPoint.GetListAllArticlePath;
            var listAllArticle = new List<Article>();
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {


                    HttpResponseMessage runResult = httpClient.GetAsync(getListAllArticle).Result;
                    if (!runResult.IsSuccessStatusCode)
                    {

                        //request failed
                        TempData["GetListAllArticleStatus"] = "Get list article Failed!";
                        return View(listAllArticle);
                    }
                    else
                    {

                        var jsonString = runResult.Content.ReadAsStringAsync().Result;
                        listAllArticle = JsonConvert.DeserializeObject<List<Article>>(jsonString);
                        return View(listAllArticle);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                TempData["GetListAllArticleStatus"] = "Can not connect to API";
                return View(listAllArticle);
            }

        }

        public ActionResult CreateCategory()
        {
            return View();
        }

        private List<Category> GetCategory()
        {
            var list = new List<Category>();
            var getListCategory = ApiEndPoint.ApiDomain + ApiEndPoint.GetListCategoryPath;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {


                    HttpResponseMessage getListResult = httpClient.GetAsync(getListCategory).Result;
                    if (!getListResult.IsSuccessStatusCode)
                    {

                        //request failed
                        Debug.WriteLine("Null list cate");

                        return list;


                    }

                    var jsonResult = getListResult.Content.ReadAsStringAsync().Result;
                    var listCate = JsonConvert.DeserializeObject<List<Category>>(jsonResult);
                    list = listCate;
                    return list;

                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);

                return list;
            }


        }


        [HttpPost]
        public ActionResult BrowserAnArticle(int id ,[Bind(Include = "Id,Title,Description,Content,Source,Link,ImgUrls,CategoryId")] Article article)
        {


            var browserArticle = ApiEndPoint.ApiDomain + ApiEndPoint.UpdateAnArticlePath;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    article.Status = Article.ArticleStatus.ACTIVE;
                    var jsonString = JsonConvert.SerializeObject(article);
                    var data = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    HttpResponseMessage result = httpClient.PutAsync(browserArticle, data).Result;
                    if (!result.IsSuccessStatusCode)
                    {

                        //request failed
                        Debug.WriteLine("failed");
                        return Redirect("/Admin/articleDetail/" + article.Id);
                    }


                    var jsonResult = result.Content.ReadAsStringAsync().Result;
                    var articleResult = JsonConvert.DeserializeObject<Article>(jsonResult);
                    TempData["Status"] = "Success :" + article.Id;

                    return Redirect("/Admin/ListPendingArticle");


                }
            }
            catch (Exception err)
            {
                TempData["Status"] = "Fail connect to api";

                Debug.WriteLine(err.Message);
                return Redirect("/Admin/articleDetail/" + article.Id);

            }
        }

        public ActionResult EditArticle(int id)
        {
            var url = ApiEndPoint.GenerateGetArticleByIdUrl(id);
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {

                    ViewBag.ListCategory = GetCategory();
                    HttpResponseMessage runResult = httpClient.GetAsync(url).Result;
                    if (!runResult.IsSuccessStatusCode)
                    {

                        //request failed
                        TempData["AritcleDetailStatus"] = "Get article detais infor failed, Id :  " + id;
                        return Redirect("/Admin/ArticleDetail/" + id);
                    }
                    else
                    {

                        var jsonString = runResult.Content.ReadAsStringAsync().Result;
                        var article = JsonConvert.DeserializeObject<Article>(jsonString);
                        return View(article);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.Message);
                Debug.WriteLine("Can not connect to API");
                TempData["AritcleDetailStatus"] = $"{err.Message} at index: {id}";
                return Redirect("/Admin/ArticleDetail/" + id);

            }

        }

        [HttpPost]
        public ActionResult EditArticle( [Bind(Include = "Id,Title,Description,Content,Source,Link,ImgUrls,CategoryId")] Article article)
        {
            if (ModelState.IsValid)
            {
                var editArticle = ApiEndPoint.ApiDomain + ApiEndPoint.UpdateAnArticlePath;
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        ViewBag.ListCategory = GetCategory();
                        var jsonString = JsonConvert.SerializeObject(article);
                        var data = new StringContent(jsonString, Encoding.UTF8, "application/json");
                        Debug.WriteLine(data);
                        HttpResponseMessage result = httpClient.PutAsync(editArticle, data).Result;
                        //if (!result.IsSuccessStatusCode )
                        //{
                        //    TempData["Status"] = "Fail to save ";
                        //    //request failed
                        //    Debug.WriteLine("[failed]");
                        //    return View(article);
                        //}


                        var jsonResult = result.Content.ReadAsStringAsync().Result;
                        if (jsonResult == null)
                        {
                            Debug.WriteLine("[null response]");
                            return View(article);
                        }
                        var articleResult = JsonConvert.DeserializeObject<Article>(jsonResult);
                        TempData["Status"] = "[Success save:]" + article.Id;
                        Debug.WriteLine("[success]");
                        return Redirect("/Admin/ArticleDetail" + articleResult.Id);


                    }
                }
                catch (Exception err)
                {
                    TempData["Status"] = "Fail connect to api";

                    Debug.WriteLine(err.Message);
                    Debug.WriteLine("[failed]");
                    return View(article);

                }
            }
            else
            {
                return View(article);

            }


        }
    }
}