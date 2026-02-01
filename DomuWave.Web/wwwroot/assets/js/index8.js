import{B as w,y as B,c as p,o as l,q as h,z as r,R as C,J as P,K as v,T as S,L as g,M as V,N as b,O as z,V as $,m as N,n as m,a as L,d as f,x as y,D as T,A as E,P as I,Q as u,w as O,E as R}from"./app.js";import{s as D}from"./index4.js";import{s as F}from"./index7.js";var W=`
    .p-tabs {
        display: flex;
        flex-direction: column;
    }

    .p-tablist {
        display: flex;
        position: relative;
    }

    .p-tabs-scrollable > .p-tablist {
        overflow: hidden;
    }

    .p-tablist-viewport {
        overflow-x: auto;
        overflow-y: hidden;
        scroll-behavior: smooth;
        scrollbar-width: none;
        overscroll-behavior: contain auto;
    }

    .p-tablist-viewport::-webkit-scrollbar {
        display: none;
    }

    .p-tablist-tab-list {
        position: relative;
        display: flex;
        background: dt('tabs.tablist.background');
        border-style: solid;
        border-color: dt('tabs.tablist.border.color');
        border-width: dt('tabs.tablist.border.width');
    }

    .p-tablist-content {
        flex-grow: 1;
    }

    .p-tablist-nav-button {
        all: unset;
        position: absolute !important;
        flex-shrink: 0;
        inset-block-start: 0;
        z-index: 2;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        background: dt('tabs.nav.button.background');
        color: dt('tabs.nav.button.color');
        width: dt('tabs.nav.button.width');
        transition:
            color dt('tabs.transition.duration'),
            outline-color dt('tabs.transition.duration'),
            box-shadow dt('tabs.transition.duration');
        box-shadow: dt('tabs.nav.button.shadow');
        outline-color: transparent;
        cursor: pointer;
    }

    .p-tablist-nav-button:focus-visible {
        z-index: 1;
        box-shadow: dt('tabs.nav.button.focus.ring.shadow');
        outline: dt('tabs.nav.button.focus.ring.width') dt('tabs.nav.button.focus.ring.style') dt('tabs.nav.button.focus.ring.color');
        outline-offset: dt('tabs.nav.button.focus.ring.offset');
    }

    .p-tablist-nav-button:hover {
        color: dt('tabs.nav.button.hover.color');
    }

    .p-tablist-prev-button {
        inset-inline-start: 0;
    }

    .p-tablist-next-button {
        inset-inline-end: 0;
    }

    .p-tablist-prev-button:dir(rtl),
    .p-tablist-next-button:dir(rtl) {
        transform: rotate(180deg);
    }

    .p-tab {
        flex-shrink: 0;
        cursor: pointer;
        user-select: none;
        position: relative;
        border-style: solid;
        white-space: nowrap;
        gap: dt('tabs.tab.gap');
        background: dt('tabs.tab.background');
        border-width: dt('tabs.tab.border.width');
        border-color: dt('tabs.tab.border.color');
        color: dt('tabs.tab.color');
        padding: dt('tabs.tab.padding');
        font-weight: dt('tabs.tab.font.weight');
        transition:
            background dt('tabs.transition.duration'),
            border-color dt('tabs.transition.duration'),
            color dt('tabs.transition.duration'),
            outline-color dt('tabs.transition.duration'),
            box-shadow dt('tabs.transition.duration');
        margin: dt('tabs.tab.margin');
        outline-color: transparent;
    }

    .p-tab:not(.p-disabled):focus-visible {
        z-index: 1;
        box-shadow: dt('tabs.tab.focus.ring.shadow');
        outline: dt('tabs.tab.focus.ring.width') dt('tabs.tab.focus.ring.style') dt('tabs.tab.focus.ring.color');
        outline-offset: dt('tabs.tab.focus.ring.offset');
    }

    .p-tab:not(.p-tab-active):not(.p-disabled):hover {
        background: dt('tabs.tab.hover.background');
        border-color: dt('tabs.tab.hover.border.color');
        color: dt('tabs.tab.hover.color');
    }

    .p-tab-active {
        background: dt('tabs.tab.active.background');
        border-color: dt('tabs.tab.active.border.color');
        color: dt('tabs.tab.active.color');
    }

    .p-tabpanels {
        background: dt('tabs.tabpanel.background');
        color: dt('tabs.tabpanel.color');
        padding: dt('tabs.tabpanel.padding');
        outline: 0 none;
    }

    .p-tabpanel:focus-visible {
        box-shadow: dt('tabs.tabpanel.focus.ring.shadow');
        outline: dt('tabs.tabpanel.focus.ring.width') dt('tabs.tabpanel.focus.ring.style') dt('tabs.tabpanel.focus.ring.color');
        outline-offset: dt('tabs.tabpanel.focus.ring.offset');
    }

    .p-tablist-active-bar {
        z-index: 1;
        display: block;
        position: absolute;
        inset-block-end: dt('tabs.active.bar.bottom');
        height: dt('tabs.active.bar.height');
        background: dt('tabs.active.bar.background');
        transition: 250ms cubic-bezier(0.35, 0, 0.25, 1);
    }
`,_={root:function(t){var a=t.props;return["p-tabs p-component",{"p-tabs-scrollable":a.scrollable}]}},H=w.extend({name:"tabs",style:W,classes:_}),U={name:"BaseTabs",extends:B,props:{value:{type:[String,Number],default:void 0},lazy:{type:Boolean,default:!1},scrollable:{type:Boolean,default:!1},showNavigators:{type:Boolean,default:!0},tabindex:{type:Number,default:0},selectOnFocus:{type:Boolean,default:!1}},style:H,provide:function(){return{$pcTabs:this,$parentInstance:this}}},M={name:"Tabs",extends:U,inheritAttrs:!1,emits:["update:value"],data:function(){return{d_value:this.value}},watch:{value:function(t){this.d_value=t}},methods:{updateValue:function(t){this.d_value!==t&&(this.d_value=t,this.$emit("update:value",t))},isVertical:function(){return this.orientation==="vertical"}}};function j(e,t,a,n,i,s){return l(),p("div",r({class:e.cx("root")},e.ptmi("root")),[h(e.$slots,"default")],16)}M.render=j;var q={root:"p-tablist",content:function(t){var a=t.instance;return["p-tablist-content",{"p-tablist-viewport":a.$pcTabs.scrollable}]},tabList:"p-tablist-tab-list",activeBar:"p-tablist-active-bar",prevButton:"p-tablist-prev-button p-tablist-nav-button",nextButton:"p-tablist-next-button p-tablist-nav-button"},J=w.extend({name:"tablist",classes:q}),Q={name:"BaseTabList",extends:B,props:{},style:J,provide:function(){return{$pcTabList:this,$parentInstance:this}}},G={name:"TabList",extends:Q,inheritAttrs:!1,inject:["$pcTabs"],data:function(){return{isPrevButtonEnabled:!1,isNextButtonEnabled:!0}},resizeObserver:void 0,watch:{showNavigators:function(t){t?this.bindResizeObserver():this.unbindResizeObserver()},activeValue:{flush:"post",handler:function(){this.updateInkBar()}}},mounted:function(){var t=this;setTimeout(function(){t.updateInkBar()},150),this.showNavigators&&(this.updateButtonState(),this.bindResizeObserver())},updated:function(){this.showNavigators&&this.updateButtonState()},beforeUnmount:function(){this.unbindResizeObserver()},methods:{onScroll:function(t){this.showNavigators&&this.updateButtonState(),t.preventDefault()},onPrevButtonClick:function(){var t=this.$refs.content,a=this.getVisibleButtonWidths(),n=v(t)-a,i=Math.abs(t.scrollLeft),s=n*.8,o=i-s,c=Math.max(o,0);t.scrollLeft=$(t)?-1*c:c},onNextButtonClick:function(){var t=this.$refs.content,a=this.getVisibleButtonWidths(),n=v(t)-a,i=Math.abs(t.scrollLeft),s=n*.8,o=i+s,c=t.scrollWidth-n,d=Math.min(o,c);t.scrollLeft=$(t)?-1*d:d},bindResizeObserver:function(){var t=this;this.resizeObserver=new ResizeObserver(function(){return t.updateButtonState()}),this.resizeObserver.observe(this.$refs.list)},unbindResizeObserver:function(){var t;(t=this.resizeObserver)===null||t===void 0||t.unobserve(this.$refs.list),this.resizeObserver=void 0},updateInkBar:function(){var t=this.$refs,a=t.content,n=t.inkbar,i=t.tabs;if(n){var s=g(a,'[data-pc-name="tab"][data-p-active="true"]');this.$pcTabs.isVertical()?(n.style.height=V(s)+"px",n.style.top=b(s).top-b(i).top+"px"):(n.style.width=z(s)+"px",n.style.left=b(s).left-b(i).left+"px")}},updateButtonState:function(){var t=this.$refs,a=t.list,n=t.content,i=n.scrollTop,s=n.scrollWidth,o=n.scrollHeight,c=n.offsetWidth,d=n.offsetHeight,x=Math.abs(n.scrollLeft),k=[v(n),S(n)],A=k[0],K=k[1];this.$pcTabs.isVertical()?(this.isPrevButtonEnabled=i!==0,this.isNextButtonEnabled=a.offsetHeight>=d&&parseInt(i)!==o-K):(this.isPrevButtonEnabled=x!==0,this.isNextButtonEnabled=a.offsetWidth>=c&&parseInt(x)!==s-A)},getVisibleButtonWidths:function(){var t=this.$refs,a=t.prevButton,n=t.nextButton,i=0;return this.showNavigators&&(i=((a==null?void 0:a.offsetWidth)||0)+((n==null?void 0:n.offsetWidth)||0)),i}},computed:{templates:function(){return this.$pcTabs.$slots},activeValue:function(){return this.$pcTabs.d_value},showNavigators:function(){return this.$pcTabs.scrollable&&this.$pcTabs.showNavigators},prevButtonAriaLabel:function(){return this.$primevue.config.locale.aria?this.$primevue.config.locale.aria.previous:void 0},nextButtonAriaLabel:function(){return this.$primevue.config.locale.aria?this.$primevue.config.locale.aria.next:void 0},dataP:function(){return P({scrollable:this.$pcTabs.scrollable})}},components:{ChevronLeftIcon:D,ChevronRightIcon:F},directives:{ripple:C}},X=["data-p"],Y=["aria-label","tabindex"],Z=["data-p"],tt=["aria-orientation"],et=["aria-label","tabindex"];function at(e,t,a,n,i,s){var o=N("ripple");return l(),p("div",r({ref:"list",class:e.cx("root"),"data-p":s.dataP},e.ptmi("root")),[s.showNavigators&&i.isPrevButtonEnabled?m((l(),p("button",r({key:0,ref:"prevButton",type:"button",class:e.cx("prevButton"),"aria-label":s.prevButtonAriaLabel,tabindex:s.$pcTabs.tabindex,onClick:t[0]||(t[0]=function(){return s.onPrevButtonClick&&s.onPrevButtonClick.apply(s,arguments)})},e.ptm("prevButton"),{"data-pc-group-section":"navigator"}),[(l(),y(T(s.templates.previcon||"ChevronLeftIcon"),r({"aria-hidden":"true"},e.ptm("prevIcon")),null,16))],16,Y)),[[o]]):L("",!0),f("div",r({ref:"content",class:e.cx("content"),onScroll:t[1]||(t[1]=function(){return s.onScroll&&s.onScroll.apply(s,arguments)}),"data-p":s.dataP},e.ptm("content")),[f("div",r({ref:"tabs",class:e.cx("tabList"),role:"tablist","aria-orientation":s.$pcTabs.orientation||"horizontal"},e.ptm("tabList")),[h(e.$slots,"default"),f("span",r({ref:"inkbar",class:e.cx("activeBar"),role:"presentation","aria-hidden":"true"},e.ptm("activeBar")),null,16)],16,tt)],16,Z),s.showNavigators&&i.isNextButtonEnabled?m((l(),p("button",r({key:1,ref:"nextButton",type:"button",class:e.cx("nextButton"),"aria-label":s.nextButtonAriaLabel,tabindex:s.$pcTabs.tabindex,onClick:t[2]||(t[2]=function(){return s.onNextButtonClick&&s.onNextButtonClick.apply(s,arguments)})},e.ptm("nextButton"),{"data-pc-group-section":"navigator"}),[(l(),y(T(s.templates.nexticon||"ChevronRightIcon"),r({"aria-hidden":"true"},e.ptm("nextIcon")),null,16))],16,et)),[[o]]):L("",!0)],16,X)}G.render=at;var nt={root:function(t){var a=t.instance,n=t.props;return["p-tab",{"p-tab-active":a.active,"p-disabled":n.disabled}]}},st=w.extend({name:"tab",classes:nt}),it={name:"BaseTab",extends:B,props:{value:{type:[String,Number],default:void 0},disabled:{type:Boolean,default:!1},as:{type:[String,Object],default:"BUTTON"},asChild:{type:Boolean,default:!1}},style:st,provide:function(){return{$pcTab:this,$parentInstance:this}}},rt={name:"Tab",extends:it,inheritAttrs:!1,inject:["$pcTabs","$pcTabList"],methods:{onFocus:function(){this.$pcTabs.selectOnFocus&&this.changeActiveValue()},onClick:function(){this.changeActiveValue()},onKeydown:function(t){switch(t.code){case"ArrowRight":this.onArrowRightKey(t);break;case"ArrowLeft":this.onArrowLeftKey(t);break;case"Home":this.onHomeKey(t);break;case"End":this.onEndKey(t);break;case"PageDown":this.onPageDownKey(t);break;case"PageUp":this.onPageUpKey(t);break;case"Enter":case"NumpadEnter":case"Space":this.onEnterKey(t);break}},onArrowRightKey:function(t){var a=this.findNextTab(t.currentTarget);a?this.changeFocusedTab(t,a):this.onHomeKey(t),t.preventDefault()},onArrowLeftKey:function(t){var a=this.findPrevTab(t.currentTarget);a?this.changeFocusedTab(t,a):this.onEndKey(t),t.preventDefault()},onHomeKey:function(t){var a=this.findFirstTab();this.changeFocusedTab(t,a),t.preventDefault()},onEndKey:function(t){var a=this.findLastTab();this.changeFocusedTab(t,a),t.preventDefault()},onPageDownKey:function(t){this.scrollInView(this.findLastTab()),t.preventDefault()},onPageUpKey:function(t){this.scrollInView(this.findFirstTab()),t.preventDefault()},onEnterKey:function(t){this.changeActiveValue(),t.preventDefault()},findNextTab:function(t){var a=arguments.length>1&&arguments[1]!==void 0?arguments[1]:!1,n=a?t:t.nextElementSibling;return n?u(n,"data-p-disabled")||u(n,"data-pc-section")==="activebar"?this.findNextTab(n):g(n,'[data-pc-name="tab"]'):null},findPrevTab:function(t){var a=arguments.length>1&&arguments[1]!==void 0?arguments[1]:!1,n=a?t:t.previousElementSibling;return n?u(n,"data-p-disabled")||u(n,"data-pc-section")==="activebar"?this.findPrevTab(n):g(n,'[data-pc-name="tab"]'):null},findFirstTab:function(){return this.findNextTab(this.$pcTabList.$refs.tabs.firstElementChild,!0)},findLastTab:function(){return this.findPrevTab(this.$pcTabList.$refs.tabs.lastElementChild,!0)},changeActiveValue:function(){this.$pcTabs.updateValue(this.value)},changeFocusedTab:function(t,a){I(a),this.scrollInView(a)},scrollInView:function(t){var a;t==null||(a=t.scrollIntoView)===null||a===void 0||a.call(t,{block:"nearest"})}},computed:{active:function(){var t;return E((t=this.$pcTabs)===null||t===void 0?void 0:t.d_value,this.value)},id:function(){var t;return"".concat((t=this.$pcTabs)===null||t===void 0?void 0:t.$id,"_tab_").concat(this.value)},ariaControls:function(){var t;return"".concat((t=this.$pcTabs)===null||t===void 0?void 0:t.$id,"_tabpanel_").concat(this.value)},attrs:function(){return r(this.asAttrs,this.a11yAttrs,this.ptmi("root",this.ptParams))},asAttrs:function(){return this.as==="BUTTON"?{type:"button",disabled:this.disabled}:void 0},a11yAttrs:function(){return{id:this.id,tabindex:this.active?this.$pcTabs.tabindex:-1,role:"tab","aria-selected":this.active,"aria-controls":this.ariaControls,"data-pc-name":"tab","data-p-disabled":this.disabled,"data-p-active":this.active,onFocus:this.onFocus,onKeydown:this.onKeydown}},ptParams:function(){return{context:{active:this.active}}},dataP:function(){return P({active:this.active})}},directives:{ripple:C}};function ot(e,t,a,n,i,s){var o=N("ripple");return e.asChild?h(e.$slots,"default",{key:1,dataP:s.dataP,class:R(e.cx("root")),active:s.active,a11yAttrs:s.a11yAttrs,onClick:s.onClick}):m((l(),y(T(e.as),r({key:0,class:e.cx("root"),"data-p":s.dataP,onClick:s.onClick},s.attrs),{default:O(function(){return[h(e.$slots,"default")]}),_:3},16,["class","data-p","onClick"])),[[o]])}rt.render=ot;export{rt as a,M as b,G as s};
