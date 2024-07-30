// 改写控制台方法
console.oldLog = console.log;
console.log = function (str) {
    console.oldLog(new Date().toLocaleTimeString() + " " + str);
}

//数学方法扩展
/**
 * 在指定范围内随机取整数
 * @version 1.1 2019年3月15日12:20:19新增
 * @author kt
 */
Math.randomInt = function (min, max) {
    return Math.floor((max - min + 1) * Math.random()) + min;
}

//字符串处理扩展
/**
 * 包含指定字符串
 * @version 1.1 2019年3月15日12:20:19新增
 * @author kt
 */
if (!String.prototype.contains) {
    String.prototype.contains = function (str) {
        return this.indexOf(str) != -1;
    }
}

/**
 * 字符串补位
 */
String.prototype.padLeft = Number.prototype.padLeft = function (total, pad) {
    return (Array(total).join(pad || 0) + this).slice(-total);
}

//集合
/**
 * IE不支持find
 * @version 1.1 2019年3月15日12:20:19新增
 * @author kt
 */
if (!Array.prototype.find) {
    Array.prototype.find = function (callback) {
        return callback && (this.filter(callback) || [])[0];
    };
}
Array.prototype.remove = function (callback) {
    var temp = callback && (this.filter(callback) || [])[0];
    if (temp!=null) {
      var index = this.indexOf(temp);
      if (index > -1) {
        this.splice(index, 1);
      }
    }
  };

//集合
/**
 * IE不支持find
 * @version 1.1 2019年3月15日12:20:19新增
 * @author kt
 */
 if (!Array.prototype.countWhere) {
    Array.prototype.countWhere = function (callback) {
        return this.filter(callback).length;
    };
}


//HTTP扩展
(function () {
    var Http = function () {

    }

    // 获取今天是一年中的第几天
    Date.prototype.GetDays = function () {
        const currentYear = new Date().getFullYear().toString();
        // 今天减今年的第一天（xxxx年01月01日）
        const hasTimestamp = new Date() - new Date(currentYear);
        // 86400000 = 24 * 60 * 60 * 1000
        let hasDays = Math.ceil(hasTimestamp / 86400000);
        return hasDays;
    }

    // 对Date的扩展，将 Date 转化为指定格式的String 
    // 月(M)、日(d)、小时(h)、分(m)、秒(s)、季度(q) 可以用 1-2 个占位符， 
    // 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字) 
    // 例子： 
    // (new Date()).Format("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423 
    // (new Date()).Format("yyyy-M-d h:m:s.S")      ==> 2006-7-2 8:9:4.18 
    Date.prototype.Format = function (fmt) {
        if (fmt == undefined) {
            fmt = 'yyyy-MM-dd hh:mm:ss';
        }
        var o = {
            "M+": this.getMonth() + 1, //月份 
            "d+": this.getDate(), //日 
            "h+": this.getHours(), //小时 
            "m+": this.getMinutes(), //分 
            "s+": this.getSeconds(), //秒 
            "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
            "S": this.getMilliseconds() //毫秒 
        };
        if (/(y+)/.test(fmt))
            fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
        for (var k in o)
            if (new RegExp("(" + k + ")").test(fmt))
                fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        return fmt;
    }

    // 对Date的扩展，将 Date 转化为指定格式的String 
    // 月(M)、日(d)、小时(h)、分(m)、秒(s)、季度(q) 可以用 1-2 个占位符， 
    // 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字) 
    // 例子： 
    // (new Date()).Format("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423 
    // (new Date()).Format("yyyy-M-d h:m:s.S")      ==> 2006-7-2 8:9:4.18 
    Date.prototype.FormatNoSecond = function (fmt) {
        if (fmt == undefined) {
            fmt = 'yyyy-MM-dd hh:mm';
        }
        var o = {
            "M+": this.getMonth() + 1, //月份 
            "d+": this.getDate(), //日 
            "h+": this.getHours(), //小时 
            "m+": this.getMinutes(), //分 
            "s+": this.getSeconds(), //秒 
            "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
            "S": this.getMilliseconds() //毫秒 
        };
        if (/(y+)/.test(fmt))
            fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
        for (var k in o)
            if (new RegExp("(" + k + ")").test(fmt))
                fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        return fmt;
    }

    Date.prototype.FormatNoTime = function (fmt) {
        if (fmt == undefined) {
            fmt = 'yyyy-MM-dd';
        }
        var o = {
            "M+": this.getMonth() + 1, //月份 
            "d+": this.getDate(), //日 
            "h+": this.getHours(), //小时 
            "m+": this.getMinutes(), //分 
            "s+": this.getSeconds(), //秒 
            "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
            "S": this.getMilliseconds() //毫秒 
        };
        if (/(y+)/.test(fmt))
            fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
        for (var k in o)
            if (new RegExp("(" + k + ")").test(fmt))
                fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        return fmt;
    }
    //时区问题
    Date.prototype.toJSON = function () {
        return this.toLocaleString();
    }

    /**
     * 获取浏览类型信息
     * @param {string} name 参数名
     * @returns 参数值
     * @version 1.1 2019年5月27日08:58:38
     * @author kt
     */
    Http.getBrowserType = function () {
        var u = navigator.userAgent;
        return {
            trident: u.indexOf('Trident') > -1, //IE内核
            presto: u.indexOf('Presto') > -1, //opera内核
            webKit: u.indexOf('AppleWebKit') > -1, //苹果、谷歌内核
            gecko: u.indexOf('Firefox') > -1, //火狐内核Gecko
            mobile: !!u.match(/AppleWebKit.*Mobile.*/), //是否为移动终端
            ios: !!u.match(/\(i[^;]+;( U;)? CPU.+Mac OS X/), //ios
            android: u.indexOf('Android') > -1 || u.indexOf('Linux') > -1, //android
            iPhone: u.indexOf('iPhone') > -1, //iPhone
            iPad: u.indexOf('iPad') > -1, //iPad
            webApp: u.indexOf('Safari') > -1 //Safari
        };
    }

    /**
     * 发送Ajax请求
     */
    Http.ajax = function (option) {
        var ts = new Date().getTime();
        var paramObejct = Http.getSignature2(option.data, ts);
        // console.log('调用接口：' + option.url + ' 参数签名：' + paramObejct.sign);
        $.ajax({
            type: option.type,
            url: option.url,
            data: paramObejct,
            headers: {
                timestamp: ts,
                signature: paramObejct.sign
            },
            success: function (reponse) {
                console.log(reponse);
                option.success(reponse);
            },
            error: option.error
        });
    }

    Http.getSignature = function (paramObejct, timeStamp) {
        var SECRET_KEY = 'A8Ea1472sflf6EDF55fEgs+';

        var paramStr = '';
        if (typeof paramObejct != "object") { // 参数必须是对象 {a:'b',c:5}  暂不再支持是a=b&c=5
            return;
        }

        var arr = [];
        for (var i in paramObejct) {
            if (paramObejct.hasOwnProperty(i)) {
                arr.push((i + paramObejct[i])); //参数值中可能带有 =  + & 等字符
            }
        }
        paramStr = arr.sort().join(""); //参数排序后拼接
        var signStr = SECRET_KEY + timeStamp + paramStr;
        var sign = md5(signStr).toUpperCase();
        return sign;
    }
    /**
     * 获取参数签名及带签名的参数对象
     * paramObejct：参数对象  必须是对象 {a:'b',c:5}  暂不再支持是a=b&c=5
     * timeStamp:时间戳
     */
    Http.getSignature2 = function (paramObejct, timeStamp) {
        var SECRET_KEY = 'A8Ea1472sflf6EDF55fEgs+';

        var paramStr = '';
        if (typeof paramObejct != "object") {
            return;
        }

        var arr = [];
        for (var i in paramObejct) {
            if (paramObejct.hasOwnProperty(i)) {
                arr.push((i + paramObejct[i])); //参数值中可能带有 =  + & 等字符
            }
        }
        paramStr = arr.sort().join(""); //参数排序后拼接
        var signStr = SECRET_KEY + timeStamp + paramStr;
        paramObejct.sign = md5(signStr).toUpperCase();

        return paramObejct;
    }
    /**
     * 获取时间戳
     */
    Http.getTimeStamp = function () {
        return new Date().getTime();
    }

    /**
     * 从页面地址获取参数值
     * @param {string} name 参数名
     * @returns 参数值
     * @version 1.1 2019年5月27日08:58:38
     * @author kt
     */
    Http.getQueryString = function (name) {
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
        var r = window.location.search.substr(1).match(reg);
        if (r != null) return (r[2]);
        return null;
    }

    /**
     * 本地存储 IE和 AppleWebkit 反其道而行
     * @param {string} cname 名称
     * @param {string} cvalue 值
     * @version 1.1 2019年5月29日09:40:38
     * @author kt
     */
    Http.setCookie = function (cname, cvalue, exdays) {
        if (exdays == undefined) {
            exdays = 1;
        }
        var type = Http.getBrowserType();
        if (type.trident) {
            var d = new Date();
            d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
            var expires = "expires=" + d.toGMTString();
            document.cookie = cname + "=" + cvalue + "; " + expires;
        } else {
            window.localStorage.setItem(cname, cvalue);
        }
    }
    /**
     * 本地存储 IE和 AppleWebkit 反其道而行
     * @param {string} cname 名称
     * @version 1.1 2019年5月29日09:40:38
     * @author kt
     */
    Http.getCookie = function (cname) {
        var type = Http.getBrowserType();
        if (type.trident) {
            var name = cname + "=";
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i].trim();
                if (c.indexOf(name) == 0) {
                    return c.substring(name.length, c.length);
                }
            }
            return null;
        } else {
            return window.localStorage.getItem(cname);
        }
    }
    /**
     * 清除cookie  
     * @version 1.1
     * @author kt
     */
    Http.deleteCookie = function (cname) {
        var type = Http.getBrowserType();
        if (type.trident) {
            this.setCookie(cname, "", -1);
        } else {
            return window.localStorage.removeItem(cname);
        }
    }
    window.Http = Http;
})();

//DOM扩展
(function () {
    var DOM = function () {

    }

    DOM.hasClass = function (element, className) {
        var names = element.className.split(/\s+/);
        for (var i = 0; i < names.length; i++) {
            if (names[i] == className) {
                return true;
            }
        }
        return false;
    }

    DOM.addClass = function (element, className) {
        if (!DOM.hasClass(element, className)) {
            element.className += ' ' + className;
        }
    }

    DOM.removeClass = function (element, className) {
        if (DOM.hasClass(element, className)) {
            var names = element.className.split(/\s+/),
                newClassName = [];
            for (var i = 0; i < names.length; i++) {
                if (names[i] != className) {
                    newClassName.push(names[i]);
                }
            }
            element.className = newClassName.join(' ');
        }
    }

    window.DOM = DOM;
})();

/**
 * 脚本资源管理器
 */
(function () {

    var ScriptManager = {
        /**
         * @private
         */
        __loadScript: function (url, callback) {
            var script = document.createElement('script');
            if (script.readyState) {
                script.onreadystatechange = function () {
                    if (script.readyState == 'loaded' || script.readyState == 'complete') {
                        callback.call();
                    }
                }
            } else {
                script.onload = callback;
            }
            script.type = 'text/javascript';
            script.src = url;
            document.getElementsByTagName('head')[0].appendChild(script);
        },
        /**
         * 加载脚本资源
         * @param {Array} urls 脚本路径集合
         * @param {Function} statechange
         */
        load: function (urls, statechange, __index) {
            __index = __index || 0;
            if (urls[__index]) {
                ScriptManager.__loadScript(urls[__index], function () {
                    ScriptManager.load(urls, statechange, __index + 1);
                });
            }
            statechange(__index);
        },

        /**
         * 加载脚本资源
         * @param {Array} url 脚本路径
         * @param {Function} 加载完成的回调
         */
        loadOne: function (url, statechange) {
            this.__loadScript(url, statechange);
        }
    }

    window.ScriptManager = ScriptManager;
})();



// 触屏、鼠标 滑动   滚动事件
window.scrollPage = {
    g_selFlag: 0,
    g_selDownPosX: 0,
    g_selDownPosY: 0,
    g_ID: "dialog", //元素ID
    init: function (id = "dialog") {
        window.scrollPage.g_ID = id;
        window.scrollPage.g_selFlag = 0;
        window.scrollPage.g_selDownPosX = 0;
        window.scrollPage.g_selDownPosY = 0;
        window.onmousedown = window.scrollPage.selDown;
        window.onmouseup = window.scrollPage.selUp;
        window.onmousemove = window.scrollPage.selMove;
    },
    selDown: function (event) {
        window.scrollPage.g_selFlag = 1;
        window.scrollPage.g_selDownPosX = event.clientX;
        window.scrollPage.g_selDownPosY = event.clientY;
        // console.log("鼠标" + 111);
    },
    selMove: function (event) {
        if (window.scrollPage.g_selFlag == 0) {
            return;
        }
        // console.log("鼠标" + 222);
        var posX = event.clientX - window.scrollPage.g_selDownPosX;
        var posY = event.clientY - window.scrollPage.g_selDownPosY;
        var dx = Math.abs(posX);
        var dy = Math.abs(posY);
        if (dy >= 5) {
            window.scrollPage.itemMove(posX, posY, dx, dy);
            window.scrollPage.g_selDownPosX = event.clientX;
            window.scrollPage.g_selDownPosY = event.clientY;
        }
    },
    selUp: function (event) {
        if (window.scrollPage.g_selFlag == 0) {
            return;
        }
        // console.log("鼠标" + 333);
        window.scrollPage.g_selFlag = 0;

        var posX = event.clientX - window.scrollPage.g_selDownPosX;
        var posY = event.clientY - window.scrollPage.g_selDownPosY;
        var dx = Math.abs(posX);
        var dy = Math.abs(posY);

        window.scrollPage.itemMove(posX, posY, dx, dy);
    },

    itemMove: function (posX, posY, dx, dy) {
        var dis = Math.sqrt(Math.pow(dx, 2) + Math.pow(dy, 2));
        //上划，下划
        var iten = document.getElementById(window.scrollPage.g_ID);
        var this_top = iten.scrollTop;
        var nScrollHeight = iten.scrollHeight;
        var nDivHeight = iten.clientHeight;
        // console.log(this_top);

        //下
        if (posY <= -5) {
            if (this_top - dy >= nScrollHeight - nDivHeight) {
                this_top = nScrollHeight - nDivHeight;
            } else {
                this_top += dy;
            }
            document.getElementById(window.scrollPage.g_ID).scrollTop = this_top;
        }

        //上
        if (posY >= 5) {
            if (this_top - dy <= 0) {
                this_top = 0;
            } else {
                this_top -= dy;
            }
            document.getElementById(window.scrollPage.g_ID).scrollTop = this_top;
        }
    }

}

window.deepClone = function (target) {
    if (target instanceof Object) {
        let dist;
        if (target instanceof Array) {
            // 拷贝数组
            dist = [];
        } else if (target instanceof Function) {
            // 拷贝函数
            dist = function () {
                return target.call(this, ...arguments);
            };
        } else if (target instanceof RegExp) {
            // 拷贝正则表达式
            dist = new RegExp(target.source, target.flags);
        } else if (target instanceof Date) {
            dist = new Date(target);
        } else {
            // 拷贝普通对象
            dist = {};
        }
        for (let key in target) {
            dist[key] = deepClone(target[key]);
        }
        return dist;
    } else {
        return target;
    }
}