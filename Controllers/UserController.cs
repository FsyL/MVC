using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mvc_BBS.Models;
using Mvc_BBS.Filter;
namespace Mvc_BBS.Controllers
{
    public class UserController : Controller
    {
        // 实例化上下文
        bbsdbEntities bdata = new bbsdbEntities();

        [CheckUser]
        public ActionResult Index()
        {
            
            //在上下文拿到板块信息，在主页面做显示
            List<boardInfo> bard = bdata.boardInfo.Where(b=>b.valid==1).ToList<boardInfo>();

            ViewData.Model = bard;
            userIinfo u = Session["user"] as userIinfo;
            userIinfo user = bdata.userIinfo.Where(t => t.id == u.id).FirstOrDefault();

            ViewBag.user = user;
            
           
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string txtName, string txtPwd)
        {
            //查询上下文对象
            userIinfo user = bdata.userIinfo.FirstOrDefault<userIinfo>(u => u.name == txtName && u.pwd == txtPwd && u.valid==1);

            //判断输入的账号密码是否存在
            if (user != null)
            {
                Session["user"] = user;

                //修改登录的最后时间
                user.login_time = DateTime.Now;
                //修改用户访问系统次数
                user.visitcount++;
                //提交数据库
                bdata.SaveChanges();

                return Content("");
            }
            else
            {
                return Content("登录失败，请检查账户密码是否正确！");
            }
        }

        /// <summary>
        /// 注册页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Reg()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Reg(string txtName, string txtPwd, int rSex, string txtEmail, string txtSigns, string txtQq)
        {
            //添加到上下文
            bdata.userIinfo.Add(new userIinfo
            {
                name = txtName,
                pwd = txtPwd,
                sex = rSex,
                email = txtEmail,
                signs = txtSigns,
                qq = txtQq,
                flag = 0,
                reg_time = DateTime.Now,
                visitcount = 0,
                psotcount = 0,
                relycount = 0,
                valid = 1,
                logo="1.jpg"
            });


            bdata.SaveChanges();

            return RedirectToAction("Regcss");
        }

        //修改个人信息
        public ActionResult UptUser()
        {
            //获取到修改对象的id值
            int uid = int.Parse(Request.QueryString["uid"]);

            userIinfo u = bdata.userIinfo.Where(t => t.id == uid).FirstOrDefault();

            ViewData.Model = u;

            return PartialView("Uptpersonal");
        }
        
        //修改个人信息模态窗口（Ajax）
        public ActionResult Uptpersonal(userIinfo u, int rSex, HttpPostedFileBase file)
        {
            userIinfo user = bdata.userIinfo.Where(t => t.id == u.id).FirstOrDefault();
            user.name = u.name;
            user.pwd = u.pwd;
            user.sex = rSex;
            user.signs = u.signs;
            user.email = u.email;
            user.qq = u.qq;

            if (file != null)
            {
                //获取到图片的后缀名
                string suffix = System.IO.Path.GetExtension(file.FileName);
                //修改图片名称
                string fname = DateTime.Now.ToString("yyyy-dd-MM-HH-mm-ss") + Guid.NewGuid().ToString();

                //新的图片名称
                string Filelogo = fname + suffix;

                //添加到自定义路径
                file.SaveAs(Server.MapPath("/LogoImg/") + Filelogo);

                //将修改好的图片添加到数据库
                user.logo = Filelogo;


            }

            //更新数据库
            bdata.SaveChanges();


            return RedirectToAction("Index");
        }

        public ActionResult Regcss()
        {
            return View();
        }
        
        /// <summary>
        /// 退出事件
        /// </summary>
        /// <returns></returns>
        public ActionResult Exit()
        {
            Session["user"] = null;

            return View("Login");
        }

        public ActionResult Post(int bid)
        {
            //拿到板块id查询到该板块下的帖子
            List<postInfo> pos = bdata.postInfo.Where(p => p.boardid == bid && p.valid==1).ToList<postInfo>();
            
            
            ViewData.Model = pos;

            return PartialView();
        }
        //一进主页循环前十条数据
        public ActionResult GetPost()
        {
            List<postInfo> pos= bdata.postInfo.Where(x=> x.valid == 1 && x.boardInfo.valid == 1).OrderByDescending(t => t.postclick).Take(10).ToList<postInfo>();

            

            ViewData.Model = pos;

            return PartialView("Post");
        }


        /// <summary>
        /// 人员信息的查询
        /// </summary>
        /// <returns></returns>
        [CheckUser]
        public ActionResult UserInfo()
        {
            //获取到session得到页面top上的值
            userIinfo u = Session["user"] as userIinfo;
            userIinfo user = bdata.userIinfo.Where(t => t.id == u.id).OrderByDescending(t=>t.valid).FirstOrDefault();
            ViewBag.user = user;

            //拿到所有人员信息
            List<userIinfo> luser = bdata.userIinfo.ToList();


            ViewData.Model = luser;

            return View();
        }

        //变更有效性
        public ActionResult CkValid(int vaid,int uid)
        {
            userIinfo u= bdata.userIinfo.Where(t => t.id == uid).FirstOrDefault();

            u.valid = vaid == 1 ? 0 : 1;

            //更新数据库
            bdata.SaveChanges();

            return RedirectToAction("UserInfo");
        }

        //重置密码
        public ActionResult ResetPwd(int uid)
        {
            userIinfo u = bdata.userIinfo.Where(t => t.id == uid).FirstOrDefault();

            u.pwd = DateTime.Now.ToString("yyyymmdd");

            //更新数据库
            bdata.SaveChanges();

            return RedirectToAction("UserInfo");
        }

        //跳转到主页面事件
        public ActionResult IndexShow()
        {
            return RedirectToAction("Index");
        }
    }
}