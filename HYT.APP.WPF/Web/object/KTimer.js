/**
* 计时器
*/
export class KTimer {
    constructor() {
        this.id = 0;
        this.lastTick=new Date();
    }

    tick = function () {
        
    }
    start = function (interval) {
        if ( interval==undefined||isNaN(interval)) {
            interval=1000;
        }
        this.tick();
        this.lastTick=new Date();
        this.id = setInterval(this.tick, interval);
    }
    stop = function () {
        clearInterval(this.id);
    }
}
