import{B as p,y as b,c as l,o as i,d as t,q as o,z as s,v as c,a as m,b as n,t as g}from"./app.js";var u=`
    .p-toolbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        flex-wrap: wrap;
        padding: dt('toolbar.padding');
        background: dt('toolbar.background');
        border: 1px solid dt('toolbar.border.color');
        color: dt('toolbar.color');
        border-radius: dt('toolbar.border.radius');
        gap: dt('toolbar.gap');
    }

    .p-toolbar-start,
    .p-toolbar-center,
    .p-toolbar-end {
        display: flex;
        align-items: center;
    }
`,v={root:"p-toolbar p-component",start:"p-toolbar-start",center:"p-toolbar-center",end:"p-toolbar-end"},y=p.extend({name:"toolbar",style:u,classes:v}),f={name:"BaseToolbar",extends:b,props:{ariaLabelledby:{type:String,default:null}},style:y,provide:function(){return{$pcToolbar:this,$parentInstance:this}}},$={name:"Toolbar",extends:f,inheritAttrs:!1},h=["aria-labelledby"];function S(e,a,d,r,M,w){return i(),l("div",s({class:e.cx("root"),role:"toolbar","aria-labelledby":e.ariaLabelledby},e.ptmi("root")),[t("div",s({class:e.cx("start")},e.ptm("start")),[o(e.$slots,"start")],16),t("div",s({class:e.cx("center")},e.ptm("center")),[o(e.$slots,"center")],16),t("div",s({class:e.cx("end")},e.ptm("end")),[o(e.$slots,"end")],16)],16,h)}$.render=S;const k={class:"container-fluid"},x={class:"text-center"},B={key:0,class:"text-gray-500 mb-0"},T=["href"],_={__name:"Error404",props:{backaction:String},setup(e){const a=c();return(d,r)=>(i(),l("div",k,[t("div",x,[r[0]||(r[0]=t("div",{class:"error mx-auto"},"404",-1)),r[1]||(r[1]=t("p",{class:"lead text-gray-800 mb-5"},"Page Not Found",-1)),n(a).visibleMessages!=null&&n(a).visibleMessages.length>0?(i(),l("p",B,g(n(a).visibleMessages[0].message),1)):m("",!0),t("a",{href:e.backaction,title:"Torna indietro",class:"btn"},"← Torna indietro",8,T)])]))}};export{_,$ as s};
