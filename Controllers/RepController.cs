using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mvc_BBS.Models;
namespace Mvc_BBS.Controllers
{
    public class RepController : Controller
    {
        bbsdbEntities bdata = new bbsdbEntities();
        

        public ActionResult Reply(int pid, string txtare)//拿到要评论帖子的id
        {
            //获得登录进来的用户信息
            userIinfo user = Session["user"] as userIinfo;

            replyInfo r = new replyInfo();
            r.postid = pid;
            r.relycontent = txtare;
            r.replytime = DateTime.Now;
            r.replyuname = user.name;
            r.valid = 1;

            //在评论后修改登录进来的用户的信息

            userIinfo u = bdata.userIinfo.Where(t => t.id == user.id).FirstOrDefault<userIinfo>();
            u.relycount++;//回帖数


            //在评论后修改帖子的一些信息
            postInfo pos = bdata.postInfo.Where(p => p.postid == pid).FirstOrDefault<postInfo>();

            pos.replycount++;//回帖数
            pos.replytime = DateTime.Now;//最后一次回帖时间
            pos.replyname = user.name;//最后一次回帖的作者

            //将评论添加到上下文
            bdata.replyInfo.Add(r);

            //提交数据库
            bdata.SaveChanges();

            //查询所有评论，传给页面
            
            ViewBag.r = r;


            return PartialView();
        }
    }
}