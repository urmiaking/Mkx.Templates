import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import vm from 'node:vm';
const source = readFileSync(new URL('../../src/Server/Mkx.Templates.Server/wwwroot/service-worker.template.js', import.meta.url), 'utf8').replace('__BUILD_ID__', 'test');
function worker() {
    const handlers = new Map(), shown = [], opened = [];
    const tab = { navigate: async path => { tab.path = path; }, focus: async () => { tab.focused = true; } };
    const self = { location:{origin:'https://example.test'},
        registration:{showNotification:async(title,options)=>{shown.push({title,options});}},
        clients:{matchAll:async()=>[tab],openWindow:async path=>{opened.push(path);}},
        addEventListener:(name,handler)=>handlers.set(name,handler) };
    vm.runInNewContext(source,{self,URL});
    const dispatch = async (name,data) => { const waits=[]; handlers.get(name)({...data,waitUntil:promise=>waits.push(promise)}); await Promise.all(waits); };
    return {shown,opened,tab,dispatch};
}
test('push renders a browser notification and routes its click within the same origin', async () => {
    const app=worker();
    await app.dispatch('push',{data:{json:()=>({title:'Meeting',body:'Reminder',url:'/meetings?id=1',tag:'event-1'})}});
    assert.equal(app.shown.length,1);
    assert.equal(app.shown[0].title,'Meeting');
    assert.equal(app.shown[0].options.body,'Reminder');
    assert.equal(app.shown[0].options.data.url,'/meetings?id=1');
    await app.dispatch('notificationclick',{notification:{data:app.shown[0].options.data,close(){}}});
    assert.equal(app.tab.path,'/meetings?id=1');
    assert.equal(app.tab.focused,true);
    assert.equal(app.opened.length,0);
});
test('notification clicks cannot redirect to another origin', async () => {
    const app=worker();
    await app.dispatch('push',{data:{json:()=>({title:'Test',url:'//attacker.invalid/phishing'})}});
    assert.equal(app.shown[0].options.data.url,'/');
});
test('a user click requests permission before network-based registration', async () => {
    const js=readFileSync(new URL('../../src/Server/Mkx.Templates.Server/wwwroot/js/push-client.js',import.meta.url),'utf8');
    const calls=[];
    const subscription={endpoint:'https://fcm.googleapis.com/fcm/send/test',toJSON:()=>({keys:{p256dh:'p256dh',auth:'auth'}})};
    const registration={pushManager:{getSubscription:async()=>null,subscribe:async()=>subscription}};
    const Notification={permission:'default',requestPermission:async()=>{calls.push('permission');Notification.permission='granted';return 'granted';}};
    const navigator={platform:'test',serviceWorker:{getRegistration:async()=>{calls.push('getRegistration');return null;},register:async()=>{calls.push('register');return registration;}}};
    const window={isSecureContext:true,PushManager:{},Notification};
    vm.runInNewContext(js,{window,navigator,Notification,localStorage:{removeItem(){}},atob,Uint8Array});
    const response=await window.mkxPush.subscribe(Buffer.alloc(65).toString('base64url'));
    assert.equal(response.subscribed,true);
    assert.deepEqual(calls,['permission','getRegistration','register']);
});
