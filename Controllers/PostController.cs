using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mvc_BBS.Models;
using Mvc_BBS.Filter;
namespace Mvc_BBS.Controllers
{
    public class PostController : Controller
    {
        bbsdbEntities bdata = new bbsdbEntities();
        // GET: Post

        /// <summary>
        /// 显示帖子详情
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public ActionResult ShowPost(int pid)
        {

            //拿到帖子id 在ShowPos详情显示
            postInfo post = bdata.postInfo.Where(p => p.postid == pid && p.valid == 1).FirstOrDefault<postInfo>();

            //点击进帖子+点击次数
            post.postclick++;
            //提交数据库
            bdata.SaveChanges();

            post.postcontent = post.postcontent.Replace(" ", "&nbsp;");
            post.postcontent = post.postcontent.Replace("   ", "&emsp;");
            post.postcontent = post.postcontent.Replace("\r\n", "<br/>");


            ViewData.Model = post;



            //查询所有评论
            List<replyInfo> rlst = bdata.replyInfo.Where(r => r.postid == pid && r.valid == 1).OrderByDescending(x => x.replytime).ToList<replyInfo>();


            ViewBag.lst = rlst;
            userIinfo u = Session["user"] as userIinfo;
            userIinfo user = bdata.userIinfo.Where(t => t.id == u.id).FirstOrDefault();
            ViewBag.user = user;

            return View();

        }
        
        //查看板块信息
        public ActionResult BodInfo()
        {
            List<boardInfo> b= bdata.boardInfo.ToList<boardInfo>();

            userIinfo u = Session["user"] as userIinfo;
            userIinfo user = bdata.userIinfo.Where(t => t.id == u.id).FirstOrDefault();
            ViewBag.user = user;
            ViewBag.lb = b;

            return View();
        }

        //添加板块
        public ActionResult AddBoard(string txtname, string txtdescription , bool valid)
        {
            //创建boarderinfo对象
            boardInfo bor = new boardInfo()
            {
                boardname = txtname,
                boarddescription = txtdescription,
                valid = valid ? 1 : 0
            };

            //添加到上下文
            bdata.boardInfo.Add(bor);
            //提交数据库
            bdata.SaveChanges();
            
            
            return Redirect("BodInfo");
        }


        //板块删除
        public ActionResult Delbar(int bid)
        {
            //拿到删除板块的id值进行删除(有效值改为无效)
            boardInfo b= bdata.boardInfo.Where(t => t.boardid == bid).FirstOrDefault<boardInfo>();

            //删除上下文(修改上下文有效性)
            b.valid = 0;

            //更新数据库
            bdata.SaveChanges();
             

            return RedirectToAction("BodInfo");
        }

        //修改板块
        public ActionResult Uptbar()
        {
            int bid = int.Parse( Request.QueryString["bid"]);
            //查询到需要修改板块
            boardInfo b = bdata.boardInfo.Where(t => t.boardid == bid).FirstOrDefault<boardInfo>();

            //将值传给Ajax页面
            ViewData.Model = b;

            return PartialView("Uptpan");
        }

        //模态修改板块信息（ajax）
        public ActionResult Uptpan(boardInfo b ,bool valide)
        {
            boardInfo bor = bdata.boardInfo.Where(t => t.boardid == b.boardid).FirstOrDefault<boardInfo>();

            bor.boardname = b.boardname;
            bor.boarddescription = b.boarddescription;
            bor.valid = valide == true ? 1 : 0;


            //更新数据库
            bdata.SaveChanges();

            return RedirectToAction("BodInfo");
        }

        //添加帖子
        [CheckUser]
        public ActionResult AddPost()
        {
            //获取到session得到页面top上的值
            userIinfo u = Session["user"] as userIinfo;
            userIinfo user = bdata.userIinfo.Where(t => t.id == u.id).FirstOrDefault();
            ViewBag.user = user;

            //下来框赋值
            List<boardInfo> lb= bdata.boardInfo.Where(t => t.valid == 1).ToList();
            List<SelectListItem> lt = new List<SelectListItem>();
            foreach (boardInfo b in lb)
            {
                SelectListItem s = new SelectListItem();
                s.Text = b.boardname;
                s.Value = b.boardid.ToString();
                s.Selected = false;
                lt.Add(s);
            }

            ViewBag.lt = lt;


            return View();
        }

        [HttpPost]
        public ActionResult AddPost(postInfo p)
        {
            //拿到session给个人信息做修改
            userIinfo u = Session["user"] as userIinfo;
            userIinfo user = bdata.userIinfo.Where(t => t.id == u.id).FirstOrDefault();

            postInfo pos = new postInfo();
            pos.boardid = p.boardid;
            pos.posttitle = p.posttitle;
            pos.postcontent = p.postcontent;
            pos.posttime = DateTime.Now;
            pos.useid = user.id;
            pos.postclick = 0;
            pos.replycount = 0;
            pos.valid = 1;

            //添加到上下文
            bdata.postInfo.Add(pos);

            //个人信息的帖子数量字段+1
            user.psotcount++;

            //更新数据库
            bdata.SaveChanges();



            return RedirectToAction("Index", "User");
        }


        /// <summary>
        /// 帖子信息管理
        /// </summary>
        /// <returns></returns>
        [CheckUser]
        public ActionResult PostInfo()
        {
            //获取到session得到页面top上的值
            userIinfo u = Session["user"] as userIinfo;
            userIinfo user = bdata.userIinfo.Where(t => t.id == u.id).FirstOrDefault();
            ViewBag.user = user;

            if (ViewData.Model==null)
            {
                List<postInfo> lp = bdata.postInfo.OrderByDescending(t => t.valid).ToList();

                ViewData.Model = lp;
                return View();
            }
            else
            {
                return View();
            }

            


        }

        
        public ActionResult DelPost(int pid ,int pvalid)
        {
            

            if (pvalid==1)
            {
                postInfo p= bdata.postInfo.Where(t => t.postid == pid).FirstOrDefault();
                p.valid = 0;

            }
            else
            {
                postInfo p = bdata.postInfo.Where(t => t.postid == pid).FirstOrDefault();
                p.valid = 1;
            }

            //更新数据库
            bdata.SaveChanges();

            return RedirectToAction("PostInfo");
        }

        /// <summary>
        /// 查询帖子
        /// </summary>
        /// <param name="txttitle"></param>
        /// <returns></returns>
        public ActionResult Postinqu(string txttitle)
        {
            

            if (string.IsNullOrWhiteSpace(txttitle))
            {
                return RedirectToAction("PostInfo");
            }
            else
            {
                //获取到session得到页面top上的值
                userIinfo u = Session["user"] as userIinfo;
                userIinfo user = bdata.userIinfo.Where(t => t.id == u.id).FirstOrDefault();
                ViewBag.user = user;

                List<postInfo> lp = bdata.postInfo.Where(t => t.posttitle.Contains(txttitle)).OrderByDescending(t=>t.valid).ToList();

                ViewData.Model = lp;
                return View("PostInfo");
            }
            
            
        }
    }
}