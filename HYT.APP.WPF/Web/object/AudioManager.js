/**
 * 音频管理类
 */
(function () {
    var AudioManager = function () {
        /**
         * 静音模式
         */
        this.mute = false;
        /**
         * buzz group对象
         */
        this.buzzGroup = null;
        /**
         * 音频列表是集合
         */
        this.list = {};
        /**
         * 状态类型
         */
        this.state = {
            NOTSTARTED: 0,
            PLAYING: 1,
            PAUSED: 2,
            ENDED: 3
        };
    }
    /**
    * 加载音频资源 
    * [ { id: 'bgm1', src: '../Resource/snd/bgm_default.mp3', loop: true }]
    */
    AudioManager.prototype.load = function (audioConfigArray) {
        var len = audioConfigArray.length;
        var group = [], item, a;

        for (var i = 0; i < len; i++) {
            item = audioConfigArray[i];
            if(item.loop==undefined){item.loop=false;}
            var volume = 100;
            if (item.volume != undefined) {
                volume = item.volume;
            }
            a = new buzz.sound(item.src, {
                formats: [],//'wav','mp3'
                preload: true,
                autoload: true,
                loop: !!item.loop,
                volume: volume,
            });

            group.push(a);
            this.list[item.id] = a;
        }

        var buzzGroup = new buzz.group(group);
        var number = 1;

        buzzGroup.bind('loadeddata', function (e) {
            if (number >= len) {

            } else {
                number++;
            }
        });
    };

    /**
     * 播放音乐
     * @param {Number} id
     * @param {Boolean} resumePlay 是否继续播放
     */
    AudioManager.prototype.play = function (id, resumePlay) {
        if (this.list[id] && !this.mute) {
            if (!resumePlay) {
                this.list[id].setTime(0);
            }
            this.list[id].play();
        }
    };
    /**
     * 循环播放音乐
     * @param {Number} id
     * @param {Boolean} resumePlay 是否是继续播放
     * @version 1.1 2019年6月14日11:05:20
     * @author kt
     */
    AudioManager.prototype.playLoop = function (id, resumePlay) {
        if (this.list[id] && !this.mute) {
            if (!resumePlay) {
                this.list[id].setTime(0);
            }
            this.list[id].loop().play();
        }
    };
    /**
     * 播放音乐 播放完回调  暂停再继续还会不会触发回调
     * @param {Number} id
     * @param {Boolean} resumePlay 是否继续播放
     * @version 1.1 2019年6月14日10:45:00
     * @author kt
     */
    AudioManager.prototype.playCallback = function (id, resumePlay, callback) {
        if (this.list[id] && !this.mute) {
            if (!resumePlay) {
                this.list[id].setTime(0);
            }

            this.list[id].bindOnce("ended", callback);
            this.list[id].play();
        }
    };
    /**
     * 暂停播放
     * @param {Number} id
     */
    AudioManager.prototype.pause = function (id) {
        if (this.list[id] != undefined) {
            this.list[id].pause();
        }
    };
    /**
     * 暂停所有音频
     */
    AudioManager.prototype.pauseAll = function () {
        //记录正在播放被暂停的音效
        this.pauseAllsounds = [];
        for (let index = 0; index < buzz.sounds.length; index++) {
            const sound = buzz.sounds[index];
            var percent = sound.getPercent();
            var ise = sound.isEnded();
            var isp = sound.isPaused();
            if (percent > 0 && percent < 100 && !ise && !isp) {//正在播放且没有结束且百分比
                this.pauseAllsounds.push(sound);
            }
        }
        buzz.all().pause();
    };
    /**
     * 恢复pauseAll 时暂停的所有音效
     */
    AudioManager.prototype.resumeAll = function () {
        if (this.pauseAllsounds && this.pauseAllsounds.length > 0) {
            for (let index = 0; index < this.pauseAllsounds.length; index++) {
                const sound = this.pauseAllsounds[index];
                sound.play();
            }
        }
    };
    AudioManager.prototype.getVolume = function (id) {
        return this.list[id].getVolume();
    };
    AudioManager.prototype.setVolume = function (id, value) {
        if (value == undefined) {
            return;
        }
        if (value <= 0) {
            value = 0;
        }
        if (value > 100) {
            value = 100;
        }
        this.list[id].setVolume(value);
    };

    /**
     * 获取音频当前状态
     * @param {*} id 
     */
    AudioManager.prototype.getCurrentSate = function (id) {
        if (this.list[id].getPercent() > 0) {
            if (this.list[id].isEnded()) {
                return this.state.ENDED;
            }
            else if (this.list[id].isPaused()) {
                return this.state.PAUSED;
            }
            else {
                return this.state.PLAYING;
            }
        } else {
            return this.state.NOTSTARTED;
        }
    }

    window.AudioManager = AudioManager;
})();
