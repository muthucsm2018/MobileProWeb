﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="MobilePro.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
     <div style='position:absolute;z-index:5;top:45%;left:45%;'>
        <img id="imgAjax" alt="loading..." title="loading..." src="AppImages/ajax-loading.gif" style="width: 100px; height: 100px" /><br /> <br />
    </div>
    <script type="text/javascript">
        /* <![CDATA[ */
        this.focus(); //focus on new window

        redirect = function () {
            //var querystring = window.location.search.substring(1); //first query string
            var page = 'http://localhost:1264/account/login';
            function toPage() {
                if (page !== undefined && page.length > 1) {
                    document.write('<!--[if !IE]>--><head><meta http-equiv="REFRESH" content="1;url=' + page + '" /><\/head><!--<![endif]-->');
                    document.write(' \n <!--[if IE]>');
                    document.write(' \n <script type="text/javascript">');
                    document.write(' \n var version = parseInt(navigator.appVersion);');
                    document.write(' \n if (version>=4 || window.location.replace) {');
                    document.write(' \n window.location.replace("' + page + '");');
                    document.write(' document.images["imgAjax"].src = "images/ajax-loading.gif"');
                    document.write(' \n } else');
                    document.write(' \n window.location.href="' + page + '";');
                    document.write(' \n  <\/script> <![endif]-->');
                }
            }
            return {
                begin: toPage
            }
        }();

        redirect.begin();

        /* ]]> */
    </script>  
    </div>
    </form>
</body>
</html>
