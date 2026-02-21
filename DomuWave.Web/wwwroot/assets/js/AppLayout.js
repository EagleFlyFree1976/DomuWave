import{B as I,h as i,j as n,q as W,v as N,x as y,m as $,G as b,M as f,F as H,_ as U,O as J,r as L,W as Z,U as Q,c as Y,k as u,L as w,E as p,H as g,J as _,K as ee,u as d,X as A,Z as C,$ as R,R as ae,A as te}from"./app.js";import{s as G,f as X,u as ne,b as j}from"./index.js";import"./api.js";var re=`
    .p-avatar {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: dt('avatar.width');
        height: dt('avatar.height');
        font-size: dt('avatar.font.size');
        background: dt('avatar.background');
        color: dt('avatar.color');
        border-radius: dt('avatar.border.radius');
    }

    .p-avatar-image {
        background: transparent;
    }

    .p-avatar-circle {
        border-radius: 50%;
    }

    .p-avatar-circle img {
        border-radius: 50%;
    }

    .p-avatar-icon {
        font-size: dt('avatar.icon.size');
        width: dt('avatar.icon.size');
        height: dt('avatar.icon.size');
    }

    .p-avatar img {
        width: 100%;
        height: 100%;
    }

    .p-avatar-lg {
        width: dt('avatar.lg.width');
        height: dt('avatar.lg.width');
        font-size: dt('avatar.lg.font.size');
    }

    .p-avatar-lg .p-avatar-icon {
        font-size: dt('avatar.lg.icon.size');
        width: dt('avatar.lg.icon.size');
        height: dt('avatar.lg.icon.size');
    }

    .p-avatar-xl {
        width: dt('avatar.xl.width');
        height: dt('avatar.xl.width');
        font-size: dt('avatar.xl.font.size');
    }

    .p-avatar-xl .p-avatar-icon {
        font-size: dt('avatar.xl.icon.size');
        width: dt('avatar.xl.icon.size');
        height: dt('avatar.xl.icon.size');
    }

    .p-avatar-group {
        display: flex;
        align-items: center;
    }

    .p-avatar-group .p-avatar + .p-avatar {
        margin-inline-start: dt('avatar.group.offset');
    }

    .p-avatar-group .p-avatar {
        border: 2px solid dt('avatar.group.border.color');
    }

    .p-avatar-group .p-avatar-lg + .p-avatar-lg {
        margin-inline-start: dt('avatar.lg.group.offset');
    }

    .p-avatar-group .p-avatar-xl + .p-avatar-xl {
        margin-inline-start: dt('avatar.xl.group.offset');
    }
`,ie={root:function(a){var r=a.props;return["p-avatar p-component",{"p-avatar-image":r.image!=null,"p-avatar-circle":r.shape==="circle","p-avatar-lg":r.size==="large","p-avatar-xl":r.size==="xlarge"}]},label:"p-avatar-label",icon:"p-avatar-icon"},se=I.extend({name:"avatar",style:re,classes:ie}),oe={name:"BaseAvatar",extends:G,props:{label:{type:String,default:null},icon:{type:String,default:null},image:{type:String,default:null},size:{type:String,default:"normal"},shape:{type:String,default:"square"},ariaLabelledby:{type:String,default:null},ariaLabel:{type:String,default:null}},style:se,provide:function(){return{$pcAvatar:this,$parentInstance:this}}};function z(e){"@babel/helpers - typeof";return z=typeof Symbol=="function"&&typeof Symbol.iterator=="symbol"?function(a){return typeof a}:function(a){return a&&typeof Symbol=="function"&&a.constructor===Symbol&&a!==Symbol.prototype?"symbol":typeof a},z(e)}function B(e,a,r){return(a=le(a))in e?Object.defineProperty(e,a,{value:r,enumerable:!0,configurable:!0,writable:!0}):e[a]=r,e}function le(e){var a=ue(e,"string");return z(a)=="symbol"?a:a+""}function ue(e,a){if(z(e)!="object"||!e)return e;var r=e[Symbol.toPrimitive];if(r!==void 0){var l=r.call(e,a);if(z(l)!="object")return l;throw new TypeError("@@toPrimitive must return a primitive value.")}return(a==="string"?String:Number)(e)}var D={name:"Avatar",extends:oe,inheritAttrs:!1,emits:["error"],methods:{onError:function(a){this.$emit("error",a)}},computed:{dataP:function(){return X(B(B({},this.shape,this.shape),this.size,this.size))}}},de=["aria-labelledby","aria-label","data-p"],pe=["data-p"],ce=["data-p"],ve=["src","alt","data-p"];function me(e,a,r,l,v,t){return n(),i("div",$({class:e.cx("root"),"aria-labelledby":e.ariaLabelledby,"aria-label":e.ariaLabel},e.ptmi("root"),{"data-p":t.dataP}),[W(e.$slots,"default",{},function(){return[e.label?(n(),i("span",$({key:0,class:e.cx("label")},e.ptm("label"),{"data-p":t.dataP}),b(e.label),17,pe)):e.$slots.icon?(n(),N(H(e.$slots.icon),{key:1,class:f(e.cx("icon"))},null,8,["class"])):e.icon?(n(),i("span",$({key:2,class:[e.cx("icon"),e.icon]},e.ptm("icon"),{"data-p":t.dataP}),null,16,ce)):e.image?(n(),i("img",$({key:3,src:e.image,alt:e.ariaLabel,onError:a[0]||(a[0]=function(){return t.onError&&t.onError.apply(t,arguments)})},e.ptm("image"),{"data-p":t.dataP}),null,16,ve)):y("",!0)]})],16,de)}D.render=me;var fe=`
    .p-skeleton {
        display: block;
        overflow: hidden;
        background: dt('skeleton.background');
        border-radius: dt('skeleton.border.radius');
    }

    .p-skeleton::after {
        content: '';
        animation: p-skeleton-animation 1.2s infinite;
        height: 100%;
        left: 0;
        position: absolute;
        right: 0;
        top: 0;
        transform: translateX(-100%);
        z-index: 1;
        background: linear-gradient(90deg, rgba(255, 255, 255, 0), dt('skeleton.animation.background'), rgba(255, 255, 255, 0));
    }

    [dir='rtl'] .p-skeleton::after {
        animation-name: p-skeleton-animation-rtl;
    }

    .p-skeleton-circle {
        border-radius: 50%;
    }

    .p-skeleton-animation-none::after {
        animation: none;
    }

    @keyframes p-skeleton-animation {
        from {
            transform: translateX(-100%);
        }
        to {
            transform: translateX(100%);
        }
    }

    @keyframes p-skeleton-animation-rtl {
        from {
            transform: translateX(100%);
        }
        to {
            transform: translateX(-100%);
        }
    }
`,he={root:{position:"relative"}},ge={root:function(a){var r=a.props;return["p-skeleton p-component",{"p-skeleton-circle":r.shape==="circle","p-skeleton-animation-none":r.animation==="none"}]}},ye=I.extend({name:"skeleton",style:fe,classes:ge,inlineStyles:he}),be={name:"BaseSkeleton",extends:G,props:{shape:{type:String,default:"rectangle"},size:{type:String,default:null},width:{type:String,default:"100%"},height:{type:String,default:"1rem"},borderRadius:{type:String,default:null},animation:{type:String,default:"wave"}},style:ye,provide:function(){return{$pcSkeleton:this,$parentInstance:this}}};function P(e){"@babel/helpers - typeof";return P=typeof Symbol=="function"&&typeof Symbol.iterator=="symbol"?function(a){return typeof a}:function(a){return a&&typeof Symbol=="function"&&a.constructor===Symbol&&a!==Symbol.prototype?"symbol":typeof a},P(e)}function ke(e,a,r){return(a=_e(a))in e?Object.defineProperty(e,a,{value:r,enumerable:!0,configurable:!0,writable:!0}):e[a]=r,e}function _e(e){var a=Se(e,"string");return P(a)=="symbol"?a:a+""}function Se(e,a){if(P(e)!="object"||!e)return e;var r=e[Symbol.toPrimitive];if(r!==void 0){var l=r.call(e,a);if(P(l)!="object")return l;throw new TypeError("@@toPrimitive must return a primitive value.")}return(a==="string"?String:Number)(e)}var M={name:"Skeleton",extends:be,inheritAttrs:!1,computed:{containerStyle:function(){return this.size?{width:this.size,height:this.size,borderRadius:this.borderRadius}:{width:this.width,height:this.height,borderRadius:this.borderRadius}},dataP:function(){return X(ke({},this.shape,this.shape))}}},we=["data-p"];function $e(e,a,r,l,v,t){return n(),i("div",$({class:e.cx("root"),style:[e.sx("root"),t.containerStyle],"aria-hidden":"true"},e.ptmi("root"),{"data-p":t.dataP}),null,16,we)}M.render=$e;const ze={class:"sidebar-header"},Pe={class:"sidebar-logo"},xe={key:0,class:"logo-text"},Ae={class:"sidebar-nav"},Ce={key:1,class:"menu-error"},Ee={key:0},Le={key:2,class:"menu-list"},Re=["onClick"],je={key:0,class:"menu-label"},Be={key:0,class:"submenu-list"},Ie={class:"menu-label"},Ne={key:0,class:"menu-label"},Ue={class:"sidebar-footer"},Ge={class:"user-info"},Xe={key:0,class:"user-details"},De={class:"user-name"},Me={class:"user-role"},Oe={__name:"AppSidebar",setup(e){const a=Q(),r=ae(),l=J(),v=ne(),t=L(!1),h=L([]);Z(async()=>{l.isAuthenticated&&v.menuItems.length===0&&(await v.fetchMenu(),O())});function O(){v.menuItems.forEach(m=>{var o;(o=m.items)!=null&&o.some(c=>c.to===a.path)&&(h.value.includes(m.key)||h.value.push(m.key))})}function V(){t.value=!t.value}function K(m){const o=h.value.indexOf(m);o>-1?h.value.splice(o,1):h.value.push(m)}function T(m){var o;return(o=m.items)==null?void 0:o.some(c=>c.to===a.path)}function q(){l.logout(),v.clearMenu(),r.push({name:"Login"})}const F=Y(()=>{var o,c;return(((o=l.currentUser)==null?void 0:o.displayName)??((c=l.currentUser)==null?void 0:c.username)??"?").split(" ").map(x=>x[0]).join("").toUpperCase().slice(0,2)});return(m,o)=>{var x;const c=ee("tooltip");return n(),i("aside",{class:f(["app-sidebar",{collapsed:t.value}])},[u("div",ze,[u("div",Pe,[o[1]||(o[1]=u("div",{class:"logo-icon"},[u("i",{class:"pi pi-building"})],-1)),p(_,{name:"fade-slide"},{default:g(()=>[t.value?y("",!0):(n(),i("div",xe,o[0]||(o[0]=[u("span",{class:"logo-name"},"DomuWave",-1),u("span",{class:"logo-sub"},"Gestione Condomini",-1)])))]),_:1})]),w(p(d(j),{icon:t.value?"pi pi-angle-right":"pi pi-angle-left",text:"",rounded:"",class:"collapse-btn",onClick:V},null,8,["icon"]),[[c,t.value?"Espandi":"Comprimi",void 0,{right:!0}]])]),u("nav",Ae,[d(v).loading?(n(),i(A,{key:0},C(5,s=>u("div",{class:"menu-skeleton",key:s},[p(d(M),{height:"2.5rem","border-radius":"10px"})])),64)):d(v).error?(n(),i("div",Ce,[o[2]||(o[2]=u("i",{class:"pi pi-exclamation-triangle"},null,-1)),t.value?y("",!0):(n(),i("span",Ee,b(d(v).error),1))])):(n(),i("ul",Le,[(n(!0),i(A,null,C(d(v).menuItems,s=>{var S,E;return n(),i("li",{key:s.key,class:f(["menu-item",{"has-children":(S=s.items)==null?void 0:S.length}])},[(E=s.items)!=null&&E.length?(n(),i(A,{key:0},[w((n(),i("button",{class:f(["menu-link menu-group-toggle",{active:T(s),open:h.value.includes(s.key)}]),onClick:k=>K(s.key)},[u("i",{class:f([s.icon,"menu-icon"])},null,2),p(_,{name:"fade-slide"},{default:g(()=>[t.value?y("",!0):(n(),i("span",je,b(s.label),1))]),_:2},1024),p(_,{name:"fade-slide"},{default:g(()=>[t.value?y("",!0):(n(),i("i",{key:0,class:f(["pi menu-chevron",h.value.includes(s.key)?"pi-chevron-down":"pi-chevron-right"])},null,2))]),_:2},1024)],10,Re)),[[c,t.value?s.label:void 0,void 0,{right:!0}]]),p(_,{name:"submenu"},{default:g(()=>[!t.value&&h.value.includes(s.key)?(n(),i("ul",Be,[(n(!0),i(A,null,C(s.items,k=>(n(),i("li",{key:k.key,class:"submenu-item"},[p(d(R),{to:k.to??"#",class:f(["menu-link submenu-link",{active:d(a).path===k.to}])},{default:g(()=>[u("i",{class:f([k.icon,"menu-icon submenu-icon"])},null,2),u("span",Ie,b(k.label),1)]),_:2},1032,["to","class"])]))),128))])):y("",!0)]),_:2},1024)],64)):w((n(),N(d(R),{key:1,to:s.to??"#",class:f(["menu-link",{active:d(a).path===s.to}])},{default:g(()=>[u("i",{class:f([s.icon,"menu-icon"])},null,2),p(_,{name:"fade-slide"},{default:g(()=>[t.value?y("",!0):(n(),i("span",Ne,b(s.label),1))]),_:2},1024)]),_:2},1032,["to","class"])),[[c,t.value?s.label:void 0,void 0,{right:!0}]])],2)}),128))]))]),u("div",Ue,[w((n(),i("div",Ge,[p(d(D),{label:F.value,shape:"circle",class:"user-avatar"},null,8,["label"]),p(_,{name:"fade-slide"},{default:g(()=>{var s,S;return[t.value?y("",!0):(n(),i("div",Xe,[u("span",De,b((s=d(l).currentUser)==null?void 0:s.displayName),1),u("span",Me,b((S=d(l).currentUser)==null?void 0:S.role),1)]))]}),_:1})])),[[c,t.value?(x=d(l).currentUser)==null?void 0:x.displayName:void 0,void 0,{right:!0}]]),w(p(d(j),{icon:"pi pi-sign-out",text:"",rounded:"",severity:"secondary",class:"logout-btn",onClick:q},null,512),[[c,"Esci",void 0,{right:!0}]])])],2)}}},Ve=U(Oe,[["__scopeId","data-v-5ebf96bf"]]),Ke={class:"app-layout"},Te={class:"app-main"},qe={__name:"AppLayout",setup(e){return(a,r)=>{const l=te("RouterView");return n(),i("div",Ke,[p(Ve),u("div",Te,[p(l)])])}}},Je=U(qe,[["__scopeId","data-v-87d0efda"]]);export{Je as default};
