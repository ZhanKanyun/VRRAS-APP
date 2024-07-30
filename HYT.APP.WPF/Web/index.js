console.log("home start");
import { DeviceManager } from './object/DeviceManager.js';
import { KTimer } from './object/KTimer.js';
window.onload = function () {


  document.onclick = function (event) {
    if (event.target.localName == 'input' && (event.target.type == "text"||event.target.type == "number")) {

      if (event.target.className == "el-input__inner") {
        console.log("点击输入框，弹出虚拟键盘");
        CSharp_App.KeyBoard_Show();
      }

      // if (event.target.className!="el-range-input") {
      //   console.log(event.target.localName + ' click');
      //   CSharp_App.KeyBoard_Show();
      // }
    }
  };



  console.log("home window.onload");
  window.DeviceManager = new DeviceManager();
  window.KTimer = KTimer;
  const {
    layer
  } = LayuiVue;
  const {
    computed,
  } = Vue;


  const { ElMessage } = ElementPlus;

  const options = {
    moduleCache: {
      vue: Vue
    },
    async getFile(url) {

      const res = await fetch(url);
      if (!res.ok)
        throw Object.assign(new Error(res.statusText + ' ' + url), { res });
      return {
        getContentData: asBinary => asBinary ? res.arrayBuffer() : res.text(),
      }
    },
    addStyle(textContent) {

      const style = Object.assign(document.createElement('style'), { textContent });
      const ref = document.head.getElementsByTagName('style')[0] || null;
      document.head.insertBefore(style, ref);
    },
  }

  const { loadModule } = window['vue3-sfc-loader'];

  const routes = [
    {
      path: '/home',
      name: 'home',
      component: () => loadModule('page/page_home.vue', options),
      meta: { keepAlive: true },
    },
    {
      path: '/',
      name: 'patientManager',
      component: () => loadModule('page/page_patientManager.vue', options),
      meta: { keepAlive: true },
    },
    {
      path: '/history',
      name: 'history',
      component: () => loadModule('page/page_record.vue', options),
      meta: { keepAlive: true },
    },
    {
      path: '/setting',
      name: 'setting',
      component: () => loadModule('page/page_setting.vue', options),
      meta: { keepAlive: true },
    },
  ]
  const router = VueRouter.createRouter({
    history: VueRouter.createWebHashHistory(),
    routes, // `routes: routes` 的缩写
  })


  const App = {
    components: {
      'btnav': Vue.defineAsyncComponent(() => loadModule('component/bt-nav.vue', options)),
    },
    setup() {
      const aa_localDB = window.aa_localDB;
      const aa_config = window.aa_config;
      return {
        layer,
        aa_localDB,
        aa_config
      }
    },
    data() {
      return {
        message: 'Vue3实例',
        currentPatient: null,
        deviceManager: null,
        dialog_trainreport_visible: false,
        train_report_id: null,
        dialog_assessreport_visible: false,
        assess_report_id: null,
        isRouterAlive: true,
        admin: null,

        deviceState: 1,//设备状态 1-正常 2-异常

        dialog_diagnosisid: null,
        dialog_diagnosisedit_visible: false,
        dialog_assessrecordcontrast_visible: false,
        dialog_assessrecordallprint_visible: false,
        dialog_assessrecordsimpleprint_visible: false,
        assessrecordcontrastArray: '',
        assessrecordAllPrintArray: '',
        assessrecordSimplePrintArray: '',


        trainset_dialog_visible: false,
        currentTrainID: '',

        current_assess_id: '',
        dialog_assess_visible: false,

        appconfig: {
          alphaVersion: "",
          isDebug: false,
        },
      }
    },
    //依赖注入
    provide() {
      return {
        admin: computed(() => this.admin)
      }
    },
    methods: {
      //刷新当前路由页面，不闪屏
      reload() {
        this.isRouterAlive = false; //先关闭，
        this.$nextTick(function () {
          this.isRouterAlive = true; //再打开
        });
      },

      //关闭评估详情
      dialog_assessreport_close: function () {
        window.m_assessData = null;
        this.dialog_assessreport_visible = false;
      },

      //关闭训练详情
      dialog_trainreport_Close: function (e) {
        this.dialog_trainreport_visible = false;
      },

      //刷新数据:从后台刷新
      getDataCacheFromCSharp: function () {
        CSharp_App.JS_GetDataCache().then(res => {
          res = JSON.parse(res);
          if (res.success) {

            this.currentPatient = res.data.patient;

            if (this.currentPatient != null) {
              console.log('[主页] 刷新数据，当前用户：' + this.currentPatient.Name);
            } else {
              console.log('[主页] 刷新数据，当前用户：无');
            }
          }
        });
      },
      
      //关闭评估
      dialog_assess_Close: function (e) {
        this.dialog_assess_visible = false;
        console.log(this.currentPatient.Data);
      },
      //打开实景训练，训练设置
      openTrain_click: function () {
        this.currentTrainID = 100001;
        this.trainset_dialog_visible = true;
      },
      //关闭实景训练，训练设置
      trainset_close: function () {
        this.trainset_dialog_visible = false;
      },
    },

    mounted: function () {
      console.log("home mounted");
      var self = this;
      window.CSharp_App = window.chrome.webview.hostObjects.CSharp_App;
      window.CSharp_DB = window.chrome.webview.hostObjects.CSharp_DB;
      window.CSharp_Device = window.chrome.webview.hostObjects.CSharp_Device;
      window.CSharp_Assess = window.chrome.webview.hostObjects.CSharp_Assess;
      this.getDataCacheFromCSharp();
      this.appconfig = window.aa_config;
      //C#接口模块：由C#代码调用
      window.API_CSharp = {
        //Vue通知
        notification: function (message, type, duration) {
          switch (type) {
            case 'success':
              self.$notify.success({
                title: '成功',
                message: message,
                duration: duration,
                // offset: 170,
              });
              break;
            case 'warning':
              self.$notify.warning({
                title: '警告',
                message: message,
                duration: duration,
                // offset: 170
              });
              break;
            case 'info':
              self.$notify.info({
                title: '消息',
                message: message,
                duration: duration,
                // offset: 170,
              });
              break;
            case 'error':
              self.$notify.error({
                title: '错误',
                message: message,
                duration: duration,
                // offset: 170,
              });
              break;
            default:
              break;
          }
        },


        setHeight: function (height) {
          document.getElementById("app").style.height = height + "px";
        },
        //紧急停止-中止评估
        stopAssess: function () {
          self.dialog_assess_visible = false;
          console.oldLog("紧急停止-中止评估。");
        },

        //评估数据更新
        accessdataupdata: function (data) {
          // console.log(data);
          self.$refs.assessPage.dataUpdata();
        },
        //查看训练详情
        opentrainreportdetail: function (id) {
          console.oldLog("查看训练详情:" + id);

          self.train_report_id = id;
          self.dialog_trainreport_visible = true;
        },
        //新增用户，自动登录，自动打开评估
        newUserAutoLoginAssess: function () {


          CSharp_App.JS_GetDataCache().then(res => {
            res = JSON.parse(res);
            if (res.success) {

              self.currentPatient = res.data.patient;

              if (window.DeviceManager.deviceState == null || !window.DeviceManager.deviceState.IsNormal) {
                self.$message.error("设备状态异常，无法自动开启评估。");
                return;
              }
              self.$confirm('是否现在进行评估?', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
              }).then(() => {
                self.current_assess_id = "scale1001";
                self.dialog_assess_visible = true;
                CSharp_App.KeyBoard_Hide();
              }).catch(() => {

              });

            }
          });

          // this.$root.getDataCacheFromCSharp();

        },

        //用户数据更新
        updatacurrentPatient: function () {
          self.getDataCacheFromCSharp();
        }
      }

      //等比缩放:自适应 分辨率及缩放
      var browerWidth = window.innerWidth; //浏览器可视宽度
      var baseWidth = 1920; //设计稿宽度
      var zoomValue = browerWidth / baseWidth; //缩放比例计算
      document.getElementById("app").style.transform = "scale(" + zoomValue + "," + zoomValue + ")"; //mainContainer为主容器id
      window.onresize = function () { //窗口尺寸变化时，重新计算和缩放
        browerWidth = window.innerWidth;
        zoomValue = browerWidth / baseWidth;
        document.getElementById("app").style.transform = "scale(" + zoomValue + "," + zoomValue + ")";
      }
    }
  }

  console.log("home createApp");
  const app = Vue.createApp(App)
  app.use(ElementPlus, {
    locale: ElementPlusLocaleZhCn,
  });
  // app.use(ElMessage);
  app.use(router)
  app.use(LayuiVue);
  // window.SpineApplication = Application;
  for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    app.component(key, component)
  }

  app.component('v-chart', VueECharts);

  app.component('bt-dialog-patientedit', Vue.defineAsyncComponent(() => loadModule('component/bt-dialog-patientedit.vue', options)));
  app.component('bt-dialog-trainset', Vue.defineAsyncComponent(() => loadModule('component/bt-dialog-trainset.vue', options)));
  app.component('bt-dialog-trainreport', Vue.defineAsyncComponent(() => loadModule('component/bt-dialog-trainreport.vue', options)));
  app.component('bt-dialog-musicmananger', Vue.defineAsyncComponent(() => loadModule('component/bt-dialog-musicmananger.vue', options)));
  app.component('bt-dialog-musicedit', Vue.defineAsyncComponent(() => loadModule('component/bt-dialog-musicedit.vue', options)));
  app.component('bt-dialog-musictrain', Vue.defineAsyncComponent(() => loadModule('component/bt-dialog-musictrain.vue', options)));

  app.component('bt-dialog-assessreport', Vue.defineAsyncComponent(() => loadModule('component/bt-dialog-assessreport.vue', options)));

  app.component('bt-dialog-assess', Vue.defineAsyncComponent(() => loadModule('component/bt-dialog-assess.vue', options)));

  app.config.unwrapInjectedRef = true// Vue 3.3 之前需要设置，以保证注入会自动解包
  app.mount('#app');
  app.config.errorHandler = (err) => {
    /* 处理错误 */
    console.log("app.config.errorHandler " + err);
  }
  console.log("home app.mount");
  window.app = app;
  window.API_CSharp.notification("启动成功", "success", 1000);
}