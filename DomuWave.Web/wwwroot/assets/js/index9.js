import{S as It,v as j,U as $,f as et,W as Re,B as me,J as ye,z as m,c as w,o as p,r as U,h as Tt,p as fe,s as Ee,l as D,a as S,b as y,d,i as T,w as I,x as C,e as K,t as A,F,R as tt,y as Ae,ad as xt,a0 as Ot,Q as ge,L as Ie,Z as Te,m as nt,n as rt,E as M,D as R,I as ot,j as Ke,q as O,ae as at,af as be,ag as Lt,ah as At,ai as Kt,aj as Dt,ak as Mt,O as Pt,al as Ft,am as Ue,an as Q,$ as Nt,P as X,ao as $t,ap as jt,aq as lt,ab as zt,ar as Vt,as as Bt,at as Rt}from"./app.js";import{u as Et}from"./accountStore.js";import{a as B}from"./systemApi.js";import{a as pe}from"./accountApi.js";import{t as te}from"./index4.js";import{a as Ut,s as ve,b as Ht}from"./index6.js";import{_ as qt}from"./Error404.js";import{c as it,a as st,b as Wt,d as dt,F as Zt}from"./index.js";import{s as J,a as Yt,d as Gt,e as Xt,f as Jt,b as Qt,c as ct}from"./index5.js";import{b as ut,s as _t}from"./index3.js";import{s as en}from"./index7.js";import{a as tn,s as nn}from"./index10.js";function He(e){return e?te(e):""}function q(e){return e||""}const rn=It("transactionStore",{state:()=>({transactions:null,selectedRows:[],pageSize:20,page:1,totalRecords:null,totalPages:null,createmode:!1,loading:!1,error:null,edititem:null,createitem:null,filterLookups:{accounts:[],categories:null,transactionTypes:[],paymentMethods:[],flowDirections:[],statuses:[],beneficiarys:[]},currentFilter:{targetAccountId:null,accountId:null,categoryId:null,fromDate:null,toDate:null,transactionType:null,flowDirection:null,status:null,note:null},massiveEditItem:{updateAccountId:!1,updateDescription:!1,updateStatus:!1,updateAmount:!1,updateCurrencyId:!1,updateBeneficiary:!1,updatePaymentMethodId:!1,updateCategoryId:!1,updateTransactionDate:!1,updateTransactionType:!1,updateDestinationAccountId:!1,accountId:null,destinationAccountId:null,transactionType:null,description:"",status:null,amount:null,currencyId:null,beneficiary:null,paymentMethodId:null,categoryId:null,transactionDate:null,TransactionIds:[]},sortBy:null,sortAscending:null}),actions:{async createEntity(e){return await this.updateEntity(e)},async updateEntity(e){var n,r;const t=j();et(),this.loading=!0,this.error=null;try{let a="/Transactions",o=null;return e.id!=0?(a=`${a}/${e.id}`,o=await B.put(a,{accountId:e.account.id,destinationAccountId:e.destinationAccount!=null?e.destinationAccount.id:null,transactionType:e.transactionType,description:e.description,status:e.status.id,amount:e.amount,currencyId:e.currency!=null?e.currency.id:null,beneficiary:e.beneficiary,paymentMethodId:e.paymentMethod.id,categoryId:e.category.id,transactionDate:te(e.transactionDate)})):(o=await B.post(a,{accountId:e.account.id,destinationAccountId:e.destinationAccount!=null?e.destinationAccount.id:null,transactionType:e.transactionType,description:e.description,status:e.status.id,amount:e.amount,currencyId:e.currency!=null?e.currency.id:null,beneficiary:e.beneficiary,paymentMethodId:e.paymentMethod.id,categoryId:e.category.id,tags:e.tags,transactionDate:te(e.transactionDate)}),this.edititem.id=o.headers["x-key"],e.id=o.headers["x-key"],this.createmode=!1),this.edititem={...e},this.createmode=!1,t.addMessage(Re.SUCCESS.SAVE,$.success),console.log("this.edititem",this.edititem),!0}catch(a){return console.log("Error",a),t.setMessages(a.response.data.Errors,$.error),this.error=((r=(n=a.response)==null?void 0:n.data)==null?void 0:r.message)||Re.ERROR.SAVE,!1}finally{this.loading=!1}},async saveMassive(e){var r,a;const t=j();this.loading=!0,this.error=null;try{e.TransactionIds=this.selectedRows;var n={accountId:e.updateAccountId&&e.account!=null?e.account.id:null,destinationAccountId:e.updateDestinationAccountId&&e.destinationAccount!=null?e.destinationAccount.id:null,transactionType:e.updateTransactionType&&e.transactionType!=null?e.transactionType:null,description:e.updateDescription&&e.description!=null?e.description:null,status:e.updateStatus&&e.status!=null?e.status.id:null,currencyId:e.updateCurrencyId&&e.currency!=null?e.currency.id:null,amount:e.updateAmount&&e.amount!=null?e.amount:null,beneficiary:e.updateBeneficiary&&e.beneficiary!=null?e.beneficiary:null,beneficiaryId:e.updateBeneficiary&&e.beneficiary!=null?e.beneficiary.id:null,paymentMethodId:e.updatePaymentMethodId&&e.paymentMethod!=null?e.paymentMethod.id:null,categoryId:e.updateCategoryId&&e.category!=null?e.category.id:null,transactionDate:e.updateTransactionDate&&e.transactionDate!=null?te(e.transactionDate):null,updateAccountId:e.updateAccountId,updateDescription:e.updateDescription,updateStatus:e.updateStatus,updateAmount:e.updateAmount,updateCurrencyId:e.updateCurrencyId,updateBeneficiary:e.updateBeneficiary,updatePaymentMethodId:e.updatePaymentMethodId,updateCategoryId:e.updateCategoryId,updateTransactionDate:e.updateTransactionDate,updateTransactionType:e.updateTransactionType,updateDestinationAccountId:e.updateDestinationAccountId,TransactionIds:this.selectedRows};console.log("messiveEdityEntity",n);const o=await B.patch("/Transactions/massive",n)}catch(o){console.log("Error",o),t.setMessages("Non è stato possibile caricare i dati",$.error),this.error=((a=(r=o.response)==null?void 0:r.data)==null?void 0:a.message)||"Errore durante il caricamento dei dati"}finally{this.loading=!1}},async newEntity(){this.createmode=!0,this.edititem={id:0,account:null,destinationAccount:null,transactionType:null,description:null,status:null,amount:null,currency:null,beneficiary:null,paymentMethod:null,category:null,tags:null,transactionDate:new Date}},async edit(e){var n,r;const t=j();this.loading=!0,this.error=null;try{const a=await B.get(`/Transactions/${e}`);this.edititem=a.data,this.createmode=!1,console.log("this.edititem",this.edititem)}catch(a){console.log("Error",a),t.setMessages("Non è stato possibile caricare i dati",$.error),this.error=((r=(n=a.response)==null?void 0:n.data)==null?void 0:r.message)||"Errore durante il caricamento dei dati"}finally{this.loading=!1}},async deleteTransaction(e){var n,r;const t=j();this.loading=!0,this.error=null;try{const a=await B.delete(`/Transactions/${e}`)}catch(a){console.log("Error",a),t.setMessages(a.response.data.Errors,$.error),this.error=((r=(n=a.response)==null?void 0:n.data)==null?void 0:r.message)||"Errore durante il caricamento dei metodi di pagamento"}finally{this.loading=!1}},async onPageChange(e){this.page=e,await this.loadAlltransaction(this.currentFilter.targetAccountId,this.currentFilter.accountId,this.currentFilter.categoryId,this.currentFilter.fromDate,this.currentFilter.toDate,this.currentFilter.transactionType,this.currentFilter.flowDirection,this.currentFilter.status,this.currentFilter.note,this.sortBy,this.sortAscending)},async filterAlltransaction(e,t,n,r,a,o,c,g,h,s,k,N){console.log("filterAlltransaction",N),this.page=N??1,this.pageSize=20,await this.loadAlltransaction(e,t,n,r,a,o,c,g,h,s,k)},async loadAlltransaction(e,t,n,r,a,o,c,g,h,s,k){var _,de;const N=j();this.loading=!0,this.error=null;try{this.currentFilter.targetAccountId=e,this.currentFilter.accountId=t,this.currentFilter.categoryId=n,this.currentFilter.fromDate=r,this.currentFilter.toDate=a,this.currentFilter.transactionType=o,this.currentFilter.flowDirection=c,this.currentFilter.status=g,this.currentFilter.note=h,this.sortBy=s,this.sortAscending=k;var z=this.page?this.page:1,l=this.pageSize?this.pageSize:20,Z=this.sortBy?this.sortBy:"",H=this.sortAscending?this.sortAscending=="asc":!0;const V=await B.get(`/Transactions/find?note=${q(this.currentFilter.note)}&status=${q(this.currentFilter.status)}&flowDirection=${q(this.currentFilter.flowDirection)}&transactionType=${q(this.currentFilter.transactionType)}&targetAccountId=${q(this.currentFilter.targetAccountId)}&accountId=${q(this.currentFilter.accountId)}&categoryId=${q(this.currentFilter.categoryId)}&fromDate=${He(this.currentFilter.fromDate)}&toDate=${He(this.currentFilter.toDate)}&page=${z}&pageSize=${l}&sortBy=${Z}&asc=${H}`);if(console.log("loadAlltransaction",this.transactions),this.transactions!=null){const Y=this.transactions.map(P=>P.id),ce=this.transactions.filter(P=>P.selected).map(P=>P.id),G=new Set(this.selectedRows);for(const P of ce)G.add(P);for(const P of Y)ce.includes(P)||G.delete(P);this.selectedRows=Array.from(G)}else this.selectedRows=[];console.log("loadAlltransaction this.selectedRows",this.selectedRows),this.transactions=V.data.items.map(Y=>({...Y,selected:this.selectedRows.includes(Y.id)})),this.pageSize=V.data.pageSize,this.page=V.data.pageNumber,this.totalRecords=V.data.totalCount,this.totalPages=V.data.totalPages}catch(V){console.log("Error",V),N.setMessages("Non è stato possibile caricare i metodi di pagamento",$.error),this.error=((de=(_=V.response)==null?void 0:_.data)==null?void 0:de.message)||"Errore durante il caricamento dei metodi di pagamento"}finally{this.loading=!1}},async loadFilterLookups(){var t,n;const e=j();this.loading=!0,this.error=null;try{const r=await pe.get("/Accounts");this.filterLookups.accounts=r.data;const a=await pe.get("/Categories?q=");this.filterLookups.categories=a.data;const o=await pe.get("/Transactions/status");this.filterLookups.statuses=o.data;const c=await pe.get("/Transactions/types");this.filterLookups.transactionTypes=c.data}catch(r){console.log("Error",r),e.setMessages("Non è stato possibile caricare gli accounts",$.error),this.error=((n=(t=r.response)==null?void 0:t.data)==null?void 0:n.message)||"Errore durante il caricamento degli accounts"}finally{this.loading=!1}},async findBeneficiaries(e){var n,r;const t=j();this.loading=!0,this.error=null;try{return(await B.get(`Beneficiaries/lookups?q=${e}&add=true`)).data}catch(a){console.log("Error",a),t.setMessages("Non è stato possibile caricare i dati",$.error),this.error=((r=(n=a.response)==null?void 0:n.data)==null?void 0:r.message)||"Errore durante il caricamento dei dati"}finally{this.loading=!1}},async findCategories(e){var n,r;const t=j();this.loading=!0,this.error=null;try{return(await B.get(`Categories/lookups?q=${e}`)).data}catch(a){console.log("Error",a),t.setMessages("Non è stato possibile caricare i dati",$.error),this.error=((r=(n=a.response)==null?void 0:n.data)==null?void 0:r.message)||"Errore durante il caricamento dei dati"}finally{this.loading=!1}},async getRate(e,t,n){var a,o;const r=j();this.loading=!0,this.error=null;try{console.log("targetDate",n);const c=await B.get(`currencies/exchange/find?targetDate=${te(n)}&toCurrencyId=${t.id}&fromCurrencyId=${e.id}`);return console.log("rate",c.data),c.data}catch(c){console.log("Error",c),r.setMessages("Non è stato possibile caricare i dati",$.error),this.error=((o=(a=c.response)==null?void 0:a.data)==null?void 0:o.message)||"Errore durante il caricamento dei dati"}finally{this.loading=!1}},async getPaymentMethods(e){var n,r;const t=j();this.loading=!0,this.error=null;try{const a=await B.get(`/Accounts/${e}/paymentmethods`);return console.log("paymentmethods",a.data),a.data}catch(a){console.log("Error",a),t.setMessages("Non è stato possibile caricare i dati",$.error),this.error=((r=(n=a.response)==null?void 0:n.data)==null?void 0:r.message)||"Errore durante il caricamento dei dati"}finally{this.loading=!1}}},computed:{}});var on=`
    .p-textarea {
        font-family: inherit;
        font-feature-settings: inherit;
        font-size: 1rem;
        color: dt('textarea.color');
        background: dt('textarea.background');
        padding-block: dt('textarea.padding.y');
        padding-inline: dt('textarea.padding.x');
        border: 1px solid dt('textarea.border.color');
        transition:
            background dt('textarea.transition.duration'),
            color dt('textarea.transition.duration'),
            border-color dt('textarea.transition.duration'),
            outline-color dt('textarea.transition.duration'),
            box-shadow dt('textarea.transition.duration');
        appearance: none;
        border-radius: dt('textarea.border.radius');
        outline-color: transparent;
        box-shadow: dt('textarea.shadow');
    }

    .p-textarea:enabled:hover {
        border-color: dt('textarea.hover.border.color');
    }

    .p-textarea:enabled:focus {
        border-color: dt('textarea.focus.border.color');
        box-shadow: dt('textarea.focus.ring.shadow');
        outline: dt('textarea.focus.ring.width') dt('textarea.focus.ring.style') dt('textarea.focus.ring.color');
        outline-offset: dt('textarea.focus.ring.offset');
    }

    .p-textarea.p-invalid {
        border-color: dt('textarea.invalid.border.color');
    }

    .p-textarea.p-variant-filled {
        background: dt('textarea.filled.background');
    }

    .p-textarea.p-variant-filled:enabled:hover {
        background: dt('textarea.filled.hover.background');
    }

    .p-textarea.p-variant-filled:enabled:focus {
        background: dt('textarea.filled.focus.background');
    }

    .p-textarea:disabled {
        opacity: 1;
        background: dt('textarea.disabled.background');
        color: dt('textarea.disabled.color');
    }

    .p-textarea::placeholder {
        color: dt('textarea.placeholder.color');
    }

    .p-textarea.p-invalid::placeholder {
        color: dt('textarea.invalid.placeholder.color');
    }

    .p-textarea-fluid {
        width: 100%;
    }

    .p-textarea-resizable {
        overflow: hidden;
        resize: none;
    }

    .p-textarea-sm {
        font-size: dt('textarea.sm.font.size');
        padding-block: dt('textarea.sm.padding.y');
        padding-inline: dt('textarea.sm.padding.x');
    }

    .p-textarea-lg {
        font-size: dt('textarea.lg.font.size');
        padding-block: dt('textarea.lg.padding.y');
        padding-inline: dt('textarea.lg.padding.x');
    }
`,an={root:function(t){var n=t.instance,r=t.props;return["p-textarea p-component",{"p-filled":n.$filled,"p-textarea-resizable ":r.autoResize,"p-textarea-sm p-inputfield-sm":r.size==="small","p-textarea-lg p-inputfield-lg":r.size==="large","p-invalid":n.$invalid,"p-variant-filled":n.$variant==="filled","p-textarea-fluid":n.$fluid}]}},ln=me.extend({name:"textarea",style:on,classes:an}),sn={name:"BaseTextarea",extends:it,props:{autoResize:Boolean},style:ln,provide:function(){return{$pcTextarea:this,$parentInstance:this}}};function ne(e){"@babel/helpers - typeof";return ne=typeof Symbol=="function"&&typeof Symbol.iterator=="symbol"?function(t){return typeof t}:function(t){return t&&typeof Symbol=="function"&&t.constructor===Symbol&&t!==Symbol.prototype?"symbol":typeof t},ne(e)}function dn(e,t,n){return(t=cn(t))in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function cn(e){var t=un(e,"string");return ne(t)=="symbol"?t:t+""}function un(e,t){if(ne(e)!="object"||!e)return e;var n=e[Symbol.toPrimitive];if(n!==void 0){var r=n.call(e,t);if(ne(r)!="object")return r;throw new TypeError("@@toPrimitive must return a primitive value.")}return(t==="string"?String:Number)(e)}var ft={name:"Textarea",extends:sn,inheritAttrs:!1,observer:null,mounted:function(){var t=this;this.autoResize&&(this.observer=new ResizeObserver(function(){requestAnimationFrame(function(){t.resize()})}),this.observer.observe(this.$el))},updated:function(){this.autoResize&&this.resize()},beforeUnmount:function(){this.observer&&this.observer.disconnect()},methods:{resize:function(){this.$el.offsetParent&&(this.$el.style.height="auto",this.$el.style.height=this.$el.scrollHeight+"px",parseFloat(this.$el.style.height)>=parseFloat(this.$el.style.maxHeight)?(this.$el.style.overflowY="scroll",this.$el.style.height=this.$el.style.maxHeight):this.$el.style.overflow="hidden")},onInput:function(t){this.autoResize&&this.resize(),this.writeValue(t.target.value,t)}},computed:{attrs:function(){return m(this.ptmi("root",{context:{filled:this.$filled,disabled:this.disabled}}),this.formField)},dataP:function(){return ye(dn({invalid:this.$invalid,fluid:this.$fluid,filled:this.$variant==="filled"},this.size,this.size))}}},fn=["value","name","disabled","aria-invalid","data-p"];function pn(e,t,n,r,a,o){return p(),w("textarea",m({class:e.cx("root"),value:e.d_value,name:e.name,disabled:e.disabled,"aria-invalid":e.invalid||void 0,"data-p":o.dataP,onInput:t[0]||(t[0]=function(){return o.onInput&&o.onInput.apply(o,arguments)})},o.attrs),null,16,fn)}ft.render=pn;function we(e){return e.map(n=>({id:n.id,code:n.name,description:n.description}))}const hn={key:0},yn={class:"card shadow mb-4"},mn={class:"card-body"},gn={class:"container-fluid mt-4"},bn={class:"row"},vn={class:"col-md-6"},wn={class:"mb-3"},kn={class:"col-md-6"},Sn={class:"mb-3"},Cn={class:"row"},In={class:"col-md-6"},Tn={class:"mb-3"},xn={class:"row mb-3"},On={class:"col-md-6"},Ln={class:"mb-3"},An={class:"col-md-6"},Kn={key:0,class:"mb-3"},Dn={class:"row mb-3"},Mn={class:"col-md-6"},Pn={class:"mb-3"},Fn={class:"col-md-6"},Nn={class:"mb-3"},$n={class:"row mb-3"},jn={class:"col-md-6"},zn={class:"mb-3"},Vn={class:"flex items-center"},Bn={class:"col-md-6"},Rn={class:"mb-3"},En={class:"flex items-center"},Un={class:"row"},Hn={class:"col-md-6"},qn={class:"mb-3"},Wn={class:"col-md-6"},Zn={class:"mb-3"},Yn={class:"row"},Gn={class:"col-md-12"},Xn={class:"mb-3"},Jn={key:1},mo={__name:"TransactionEditForm",setup(e,{expose:t}){var n=U(!1);const r=U([]),a=U([]),o=U([]),c=async u=>{console.log("Carico le valute",u.query),r.value=await z.loadCurrencies(u.query),console.log("Ho caricato le valute:",r)},g=async u=>{var i=await s.findBeneficiaries(u.query);console.log("find",u.query,i),(i==null||i.length==0)&&(i=[{name:" "+u.query,description:" "+u.query,id:22}]),console.log("find 2",i),a.value=i,console.log(a.value)},h=async u=>{var i=await s.findCategories(u.query);console.log("find category",u.query,i),o.value=i},s=rn(),k=Et(),N=j(),z=et();var l=U({...s.edititem});const Z=U(null),H=U(null),_=U(!1);console.log("editEntity ",l);const de=({values:u})=>{console.log("RESOLVER",u);const i={};return console.log("editEntity.value.transactionType",l.value.transactionType),(l.value.transactionType==null||l.value.transactionType=="")&&l.value.transactionType!=0&&(i.transactionType=[{message:"Specificare il tipo"}]),l.value.account==null&&(i.currency=[{message:"Selezionare l'account"}]),l.value.transactionType==2&&l.value.destinationAccount==null&&(i.accountTo=[{message:"Specificare l'account di destinazione"}]),l.value.paymentMethod==null&&(i.paymentmethod=[{message:"Selezionare il metodo di pagamento."}]),l.value.currency==null&&(i.currency=[{message:"Selezionare la valuta di riferimento."}]),console.log("editEntity.value.validFrom ",l.value.transactionDate),(l.value.transactionDate==null||l.value.transactionDate=="")&&(i.transactionDate=[{message:"Selezionare la data di riferimento."}]),l.value.rate==null&&(i.rate=[{message:"Specificare il tasso di cambio."}]),{errors:i}};console.log("onFormSubmit pre");async function V(){return new Promise(async u=>{H.value?(Z.value=u,await H.value.submit()):u(!1)})}const Y=async({valid:u})=>{let i=!1;if(_.value=!0,u){n.value=!0;try{s.createmode?i=await s.createEntity(l.value):i=await s.updateEntity(l.value)}catch(v){console.error("Errore nel salvataggio:",v),N.showError("Errore durante il salvataggio del cambio valuta."),i=!1}finally{n.value=!1}}Z.value&&(console.log("onFormSubmit resolve",i),Z.value(i),console.log("onFormSubmit resolve 2",i),Z.value=null)};console.log("onFormSubmit after");async function ce(){if(l!=null&&l.value!=null){var u=l.value.account;if(u!=null){var i=await s.getPaymentMethods(u.id);s.filterLookups.paymentMethods=we(i.map(v=>v.paymentMethod))}}}async function G(u){if(u!=null){console.log("CCC",u),console.log("CCC editEntity.beneficiary",l);var i=u.value;console.log("BBBB",i),i!=null?(l.value.beneficiary=i,l.value.beneficiaryId=i.id,i.category!=null,l.value.category=i.category):(l.value.beneficiary=null,l.value.beneficiaryId=null)}}async function P(u){if(u!=null){var i=u.value;console.log("BBBB",i.value),i!=null?l.value.category=i:l.value.category=null}}async function bt(u){const i=k.accounts.find(x=>x.id===u.value.id);if(i!=null){var v=l!=null&&l.value!=null?l.value.account:null;console.log("fromAccount: ",v);const x=v!=null?k.accounts.find(L=>L.id===v.id):null;if(console.log("fromRawAccount: ",x),l.value.currency=i.currency,x!=null){var b=await s.getRate(x.currency,i.currency,l.value.transactionDate);b!=null?l.value.rate=b.rate:l.value.rate=null}}}async function vt(){l.value.transactionType!=null&&l.value.transactionType!=2&&(l.value.destinationAccount=null);var u=l!=null&&l.value!=null?l.account:null,i=l!=null&&l.value!=null?l.value.destinationAccount:null;if(i!=null){const b=k.accounts.find(L=>L.id===i.id),x=u!=null?k.accounts.find(L=>L.id===u.id):null;if(l.value.currency=b.currency,x!=null){var v=await s.getRate(x.currency,b.currency,l.value.transactionDate);v!=null?l.value.rate=v.rate:l.value.rate=null}}}async function wt(u){const i=k.accounts.find(L=>L.id===u.value.id);if(i!=null){var v=await s.getPaymentMethods(i.id);if(s.filterLookups.paymentMethods=we(v.map(L=>L.paymentMethod)),console.log("isTrasfer",E.value),!E.value)console.log("editEntity.value.currency",l.value.currency),l.value.currency=i.currency,console.log("editEntity.value.currency new",l.value.currency),l.value.rate=1;else{var b=l!=null&&l.value!=null?l.destinationAccount:null;if(b!=null){const L=k.accounts.find(ue=>ue.id===b.id);console.log("onaccountchange call getrate",i.currency,L,l.value.transactionDate);var x=await s.getRate(i.currency,L.currency,l.value.transactionDate);x!=null?l.value.rate=x.rate:l.value.rate=null}}}}async function kt(u){var i=l.value!=null?l.value.currency:null,v=l.value!=null?l.value.account.currency:null;if(i!=null){var b=await s.getRate(v,i,u);b!=null?l.value.rate=b.rate:l.value.rate=null}else l.value.rate=null}async function De(u){if(u!=null&&u.value!=null){var i=l.value!=null&&l.value.account!=null?l.value.account.currency:null;if(l.value!=null&&l.value.account==null)return;if(i==null){console.log("accountCurrency certo rawacc",l.value.account.id);var v=k.accounts.find(x=>x.id===l.value.account.id);console.log("accountCurrency certo rawacc",v),i=v.currency}console.log("accountCurrency ",i),console.log("accountCurrency evt.value",u.value);var b=await s.getRate(i,u.value,l.value.transactionDate);b!=null?l.value.rate=b.rate:l.value.rate=null}else l.value.rate=null}Tt(async()=>{await ce()}),t({submitForm:V});const Me=fe(()=>we(k.accounts)),E=fe(()=>l!=null&&l.value.transactionType!=null&&l.value.transactionType==2),St=fe(()=>{console.log("enableCurrency");var u=l!=null&&l.value!=null&&l.value.currency!=null&&l.value.currency!=""&&!E.value;return console.log("enableCurrency",u),u}),Ct=fe(()=>{var u=null,i=null;if(console.log("Enable RATE ",l.rate),l.rate==null)return!0;l!=null&&l.value!=null&&l.value.account!=null&&(u=k.accounts.find(L=>L.id===l.value.account.id)),E&&l!=null&&l.value!=null&&l.value.destinationAccount!=null&&(i=k.accounts.find(L=>L.id===l.value.destinationAccount.id));var v=!1;console.log("enableRate fromAccount",u),console.log("enableRate toAccount",i),u=u!=null&&u.value!=null?u.value:u,i=i!=null&&i.value!=null?i.value:i,console.log("enableRate",u,i),console.log("enableRate c",u!=null?u.currency:null,i!=null?i.currency:null);var b=u!=null?u.currency:null,x=i!=null?i.currency:null;return b==null||E.value&&x==null?(console.log("enableRate currencyFrom",b),console.log("enableRate isTransfer",E.value),!1):(!E.value&&l.value.currency!=null&&(x=l.value.currency),v=b.id!=x.id||l.rate==null,console.log("enableRate result:",v),console.log("enableRate editEntity.rate:",l.rate),v)});return Ee(l,async()=>{_.value&&H.value&&await H.value.validate()},{deep:!0}),Ee(()=>s.edititem,u=>{u!=null&&(l.value={...s.edititem})},{immediate:!0}),(u,i)=>{const v=D("Message");return p(),w(F,null,[y(s).edititem!=null?(p(),w("div",hn,[d("div",yn,[d("div",mn,[d("div",gn,[d("div",null,[T(y(Wt),{ref_key:"formRef",ref:H,resolver:de,onSubmit:Y,class:"flex flex-col gap-4 w-full sm:w-56 mt-2 ml-2"},{default:I(b=>{var x,L,ue,Pe,Fe,Ne,$e,je,ze,Ve,Be;return[d("div",bn,[d("div",vn,[d("div",wn,[i[12]||(i[12]=d("label",{for:"transactionDate",class:"col-form-label"}," Data ",-1)),d("div",null,[T(y(Ut),{name:"transactionDate",fluid:"",modelValue:y(l).transactionDate,"onUpdate:modelValue":[i[0]||(i[0]=f=>y(l).transactionDate=f),kt],dateFormat:y(z).options.dateFormat,placeholder:"Seleziona la data",class:"w-100"},null,8,["modelValue","dateFormat"]),(x=b.transactionDate)!=null&&x.invalid?(p(),C(v,{key:0,severity:"error",size:"small",variant:"simple"},{default:I(()=>{var f;return[K(A((f=b.transactionDate.error)==null?void 0:f.message),1)]}),_:2},1024)):S("",!0)])])]),d("div",kn,[d("div",Sn,[i[13]||(i[13]=d("label",{for:"status",class:"col-form-label"},"Stato",-1)),d("div",null,[T(y(J),{name:"status",modelValue:y(l).status,"onUpdate:modelValue":i[1]||(i[1]=f=>y(l).status=f),options:y(s).filterLookups.statuses,optionLabel:"description",placeholder:"Seleziona lo stato",class:"w-100"},null,8,["modelValue","options"]),(L=b.status)!=null&&L.invalid?(p(),C(v,{key:0,severity:"error",size:"small",variant:"simple"},{default:I(()=>{var f;return[K(A((f=b.status.error)==null?void 0:f.message),1)]}),_:2},1024)):S("",!0)])])])]),d("div",Cn,[d("div",In,[d("div",Tn,[i[14]||(i[14]=d("label",{for:"transactionType",class:"col-form-label"},"Tipo",-1)),d("div",null,[T(y(J),{name:"transactionType",modelValue:y(l).transactionType,"onUpdate:modelValue":i[2]||(i[2]=f=>y(l).transactionType=f),options:y(s).filterLookups.transactionTypes,optionLabel:"description",optionValue:"id",placeholder:"Seleziona il tipo",onChange:vt,class:"w-100"},null,8,["modelValue","options"]),(ue=b.transactionType)!=null&&ue.invalid?(p(),C(v,{key:0,severity:"error",size:"small",variant:"simple"},{default:I(()=>{var f;return[K(A((f=b.transactionType.error)==null?void 0:f.message),1)]}),_:2},1024)):S("",!0)])])])]),d("div",xn,[d("div",On,[d("div",Ln,[i[15]||(i[15]=d("label",{for:"account",class:"col-form-label"},"Account",-1)),d("div",null,[T(y(J),{name:"account",modelValue:y(l).account,"onUpdate:modelValue":i[3]||(i[3]=f=>y(l).account=f),options:Me.value,optionLabel:"code",placeholder:"Seleziona l'account",onChange:wt,class:"w-100"},null,8,["modelValue","options"]),(Pe=b.account)!=null&&Pe.invalid?(p(),C(v,{key:0,severity:"error",size:"small",variant:"simple"},{default:I(()=>{var f;return[K(A((f=b.account.error)==null?void 0:f.message),1)]}),_:2},1024)):S("",!0)])])]),d("div",An,[E.value?(p(),w("div",Kn,[i[16]||(i[16]=d("label",{for:"accountTo",class:"col-form-label"},"Account a",-1)),d("div",null,[T(y(J),{name:"accountTo",modelValue:y(l).destinationAccount,"onUpdate:modelValue":i[4]||(i[4]=f=>y(l).destinationAccount=f),options:Me.value,optionLabel:"code",placeholder:"Seleziona l'account di destinazione",onChange:bt,class:"w-100"},null,8,["modelValue","options"]),(Fe=b.accountTo)!=null&&Fe.invalid?(p(),C(v,{key:0,severity:"error",size:"small",variant:"simple"},{default:I(()=>{var f;return[K(A((f=b.accountTo.error)==null?void 0:f.message),1)]}),_:2},1024)):S("",!0)])])):S("",!0)])]),d("div",Dn,[d("div",Mn,[d("div",Pn,[i[17]||(i[17]=d("label",{for:"paymentmethod",class:"col-form-label"},"Metodo di pagamento",-1)),d("div",null,[T(y(J),{name:"paymentmethod",modelValue:y(l).paymentMethod,"onUpdate:modelValue":i[5]||(i[5]=f=>y(l).paymentMethod=f),options:y(s).filterLookups.paymentMethods,optionLabel:"description",placeholder:"Seleziona il metodo di pagamento",class:"w-100"},null,8,["modelValue","options"]),(Ne=b.paymentmethod)!=null&&Ne.invalid?(p(),C(v,{key:0,severity:"error",size:"small",variant:"simple"},{default:I(()=>{var f;return[K(A((f=b.paymentmethod.error)==null?void 0:f.message),1)]}),_:2},1024)):S("",!0)])])]),d("div",Fn,[d("div",Nn,[i[18]||(i[18]=d("label",{for:"amount",class:"col-form-label"},"Importo",-1)),d("div",null,[T(y(Yt),{modelValue:y(l).amount,"onUpdate:modelValue":i[6]||(i[6]=f=>y(l).amount=f),min:0,step:.01,useGrouping:!0,minFractionDigits:2,maxFractionDigits:2,mode:"currency",currency:y(l).currency!=null&&y(l).currency!=""?y(l).currency.code:"EUR",locale:"it-IT",showButtons:"",class:"w-100"},null,8,["modelValue","currency"]),($e=b.amount)!=null&&$e.invalid?(p(),C(v,{key:0,severity:"error",size:"small",variant:"simple"},{default:I(()=>{var f;return[K(A((f=b.type.amount)==null?void 0:f.message),1)]}),_:2},1024)):S("",!0)])])])]),d("div",$n,[d("div",jn,[d("div",zn,[i[19]||(i[19]=d("label",{for:"beneficiary",class:"col-form-label"},"Beneficiario",-1)),d("div",null,[T(y(ve),{name:"beneficiary",optionLabel:"description",modelValue:y(l).beneficiary,"onUpdate:modelValue":[i[7]||(i[7]=f=>y(l).beneficiary=f),G],suggestions:a.value,forceSelection:"false",fluid:"",minLength:"2",onItemSelect:G,onComplete:g,placeholder:"Seleziona il beneficiario",class:"w-100"},{option:I(f=>[d("div",Vn,[d("div",null,A(f.option.description),1)])]),_:1},8,["modelValue","suggestions"]),(je=b.beneficiary)!=null&&je.invalid?(p(),C(v,{key:0,severity:"error",size:"small",variant:"simple"},{default:I(()=>{var f;return[K(A((f=b.beneficiary.error)==null?void 0:f.message),1)]}),_:2},1024)):S("",!0)])])]),d("div",Bn,[d("div",Rn,[i[20]||(i[20]=d("label",{for:"category",class:"col-form-label"},"Categoria",-1)),d("div",null,[T(y(ve),{name:"category",optionLabel:"description",modelValue:y(l).category,"onUpdate:modelValue":[i[8]||(i[8]=f=>y(l).category=f),P],suggestions:o.value,forceSelection:"false",fluid:"",minLength:"2",onItemSelect:P,onComplete:h,placeholder:"Seleziona la categoria",class:"w-100"},{option:I(f=>[d("div",En,[d("div",null,A(f.option.description),1)])]),_:1},8,["modelValue","suggestions"]),(ze=b.category)!=null&&ze.invalid?(p(),C(v,{key:0,severity:"error",size:"small",variant:"simple"},{default:I(()=>{var f;return[K(A((f=b.category.error)==null?void 0:f.message),1)]}),_:2},1024)):S("",!0)])])])]),d("div",Un,[d("div",Hn,[d("div",qn,[i[21]||(i[21]=d("label",{for:"name",class:"col-form-label"},"Valuta",-1)),d("div",null,[T(y(ve),{name:"currency",optionLabel:"description",modelValue:y(l).currency,"onUpdate:modelValue":[i[9]||(i[9]=f=>y(l).currency=f),De],suggestions:r.value,fluid:"",minLength:"2",disabled:!St.value,onItemSelect:De,onComplete:c,placeholder:"Seleziona la valuta",class:"w-100"},null,8,["modelValue","suggestions","disabled"]),(Ve=b.currency)!=null&&Ve.invalid?(p(),C(v,{key:0,severity:"error",size:"small",variant:"simple"},{default:I(()=>{var f;return[K(A((f=b.currency.error)==null?void 0:f.message),1)]}),_:2},1024)):S("",!0)])])]),d("div",Wn,[d("div",Zn,[i[22]||(i[22]=d("label",{for:"rate",class:"col-form-label"}," Tasso ",-1)),d("div",null,[T(y(st),{name:"rate",type:"number",disabled:!Ct.value,placeholder:"Specifica il tasso",fluid:"",modelValue:y(l).rate,"onUpdate:modelValue":i[10]||(i[10]=f=>y(l).rate=f),class:"w-100"},null,8,["disabled","modelValue"]),(Be=b.rate)!=null&&Be.invalid?(p(),C(v,{key:0,severity:"error",size:"small",variant:"simple"},{default:I(()=>{var f;return[K(A((f=b.rate.error)==null?void 0:f.message),1)]}),_:2},1024)):S("",!0)])])])]),d("div",Yn,[d("div",Gn,[d("div",Xn,[i[23]||(i[23]=d("label",{for:"name",class:"col-form-label"},"Note",-1)),d("div",null,[T(y(ft),{modelValue:y(l).description,"onUpdate:modelValue":i[11]||(i[11]=f=>y(l).description=f),rows:"5",cols:"30",class:"w-100"},null,8,["modelValue"])])])])])]}),_:1},512)])])])])])):S("",!0),y(s).edititem==null&&!y(z).loading?(p(),w("div",Jn,[T(qt,{backaction:/PaymentMethods/})])):S("",!0)],64)}}};var Qn=`
    .p-tree {
        display: block;
        background: dt('tree.background');
        color: dt('tree.color');
        padding: dt('tree.padding');
    }

    .p-tree-root-children,
    .p-tree-node-children {
        display: flex;
        list-style-type: none;
        flex-direction: column;
        margin: 0;
        gap: dt('tree.gap');
    }

    .p-tree-root-children {
        padding: 0;
        padding-block-start: dt('tree.gap');
    }

    .p-tree-node-children {
        padding: 0;
        padding-block-start: dt('tree.gap');
        padding-inline-start: dt('tree.indent');
    }

    .p-tree-node {
        padding: 0;
        outline: 0 none;
    }

    .p-tree-node-content {
        border-radius: dt('tree.node.border.radius');
        padding: dt('tree.node.padding');
        display: flex;
        align-items: center;
        outline-color: transparent;
        color: dt('tree.node.color');
        gap: dt('tree.node.gap');
        transition:
            background dt('tree.transition.duration'),
            color dt('tree.transition.duration'),
            outline-color dt('tree.transition.duration'),
            box-shadow dt('tree.transition.duration');
    }

    .p-tree-node:focus-visible > .p-tree-node-content {
        box-shadow: dt('tree.node.focus.ring.shadow');
        outline: dt('tree.node.focus.ring.width') dt('tree.node.focus.ring.style') dt('tree.node.focus.ring.color');
        outline-offset: dt('tree.node.focus.ring.offset');
    }

    .p-tree-node-content.p-tree-node-selectable:not(.p-tree-node-selected):hover {
        background: dt('tree.node.hover.background');
        color: dt('tree.node.hover.color');
    }

    .p-tree-node-content.p-tree-node-selectable:not(.p-tree-node-selected):hover .p-tree-node-icon {
        color: dt('tree.node.icon.hover.color');
    }

    .p-tree-node-content.p-tree-node-selected {
        background: dt('tree.node.selected.background');
        color: dt('tree.node.selected.color');
    }

    .p-tree-node-content.p-tree-node-selected .p-tree-node-toggle-button {
        color: inherit;
    }

    .p-tree-node-toggle-button {
        cursor: pointer;
        user-select: none;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        overflow: hidden;
        position: relative;
        flex-shrink: 0;
        width: dt('tree.node.toggle.button.size');
        height: dt('tree.node.toggle.button.size');
        color: dt('tree.node.toggle.button.color');
        border: 0 none;
        background: transparent;
        border-radius: dt('tree.node.toggle.button.border.radius');
        transition:
            background dt('tree.transition.duration'),
            color dt('tree.transition.duration'),
            border-color dt('tree.transition.duration'),
            outline-color dt('tree.transition.duration'),
            box-shadow dt('tree.transition.duration');
        outline-color: transparent;
        padding: 0;
    }

    .p-tree-node-toggle-button:enabled:hover {
        background: dt('tree.node.toggle.button.hover.background');
        color: dt('tree.node.toggle.button.hover.color');
    }

    .p-tree-node-content.p-tree-node-selected .p-tree-node-toggle-button:hover {
        background: dt('tree.node.toggle.button.selected.hover.background');
        color: dt('tree.node.toggle.button.selected.hover.color');
    }

    .p-tree-root {
        overflow: auto;
    }

    .p-tree-node-selectable {
        cursor: pointer;
        user-select: none;
    }

    .p-tree-node-leaf > .p-tree-node-content .p-tree-node-toggle-button {
        visibility: hidden;
    }

    .p-tree-node-icon {
        color: dt('tree.node.icon.color');
        transition: color dt('tree.transition.duration');
    }

    .p-tree-node-content.p-tree-node-selected .p-tree-node-icon {
        color: dt('tree.node.icon.selected.color');
    }

    .p-tree-filter {
        margin: dt('tree.filter.margin');
    }

    .p-tree-filter-input {
        width: 100%;
    }

    .p-tree-loading {
        position: relative;
        height: 100%;
    }

    .p-tree-loading-icon {
        font-size: dt('tree.loading.icon.size');
        width: dt('tree.loading.icon.size');
        height: dt('tree.loading.icon.size');
    }

    .p-tree .p-tree-mask {
        position: absolute;
        z-index: 1;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .p-tree-flex-scrollable {
        display: flex;
        flex: 1;
        height: 100%;
        flex-direction: column;
    }

    .p-tree-flex-scrollable .p-tree-root {
        flex: 1;
    }
`,_n={root:function(t){var n=t.props;return["p-tree p-component",{"p-tree-selectable":n.selectionMode!=null,"p-tree-loading":n.loading,"p-tree-flex-scrollable":n.scrollHeight==="flex"}]},mask:"p-tree-mask p-overlay-mask",loadingIcon:"p-tree-loading-icon",pcFilterContainer:"p-tree-filter",pcFilterInput:"p-tree-filter-input",wrapper:"p-tree-root",rootChildren:"p-tree-root-children",node:function(t){var n=t.instance;return["p-tree-node",{"p-tree-node-leaf":n.leaf}]},nodeContent:function(t){var n=t.instance;return["p-tree-node-content",n.node.styleClass,{"p-tree-node-selectable":n.selectable,"p-tree-node-selected":n.checkboxMode&&n.$parentInstance.highlightOnSelect?n.checked:n.selected}]},nodeToggleButton:"p-tree-node-toggle-button",nodeToggleIcon:"p-tree-node-toggle-icon",nodeCheckbox:"p-tree-node-checkbox",nodeIcon:"p-tree-node-icon",nodeLabel:"p-tree-node-label",nodeChildren:"p-tree-node-children"},er=me.extend({name:"tree",style:Qn,classes:_n}),tr={name:"BaseTree",extends:Ae,props:{value:{type:null,default:null},expandedKeys:{type:null,default:null},selectionKeys:{type:null,default:null},selectionMode:{type:String,default:null},metaKeySelection:{type:Boolean,default:!1},loading:{type:Boolean,default:!1},loadingIcon:{type:String,default:void 0},loadingMode:{type:String,default:"mask"},filter:{type:Boolean,default:!1},filterBy:{type:[String,Function],default:"label"},filterMode:{type:String,default:"lenient"},filterPlaceholder:{type:String,default:null},filterLocale:{type:String,default:void 0},highlightOnSelect:{type:Boolean,default:!1},scrollHeight:{type:String,default:null},level:{type:Number,default:0},ariaLabelledby:{type:String,default:null},ariaLabel:{type:String,default:null}},style:er,provide:function(){return{$pcTree:this,$parentInstance:this}}};function re(e){"@babel/helpers - typeof";return re=typeof Symbol=="function"&&typeof Symbol.iterator=="symbol"?function(t){return typeof t}:function(t){return t&&typeof Symbol=="function"&&t.constructor===Symbol&&t!==Symbol.prototype?"symbol":typeof t},re(e)}function qe(e,t){var n=typeof Symbol<"u"&&e[Symbol.iterator]||e["@@iterator"];if(!n){if(Array.isArray(e)||(n=pt(e))||t){n&&(e=n);var r=0,a=function(){};return{s:a,n:function(){return r>=e.length?{done:!0}:{done:!1,value:e[r++]}},e:function(s){throw s},f:a}}throw new TypeError(`Invalid attempt to iterate non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`)}var o,c=!0,g=!1;return{s:function(){n=n.call(e)},n:function(){var s=n.next();return c=s.done,s},e:function(s){g=!0,o=s},f:function(){try{c||n.return==null||n.return()}finally{if(g)throw o}}}}function We(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter(function(a){return Object.getOwnPropertyDescriptor(e,a).enumerable})),n.push.apply(n,r)}return n}function Ze(e){for(var t=1;t<arguments.length;t++){var n=arguments[t]!=null?arguments[t]:{};t%2?We(Object(n),!0).forEach(function(r){nr(e,r,n[r])}):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):We(Object(n)).forEach(function(r){Object.defineProperty(e,r,Object.getOwnPropertyDescriptor(n,r))})}return e}function nr(e,t,n){return(t=rr(t))in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function rr(e){var t=or(e,"string");return re(t)=="symbol"?t:t+""}function or(e,t){if(re(e)!="object"||!e)return e;var n=e[Symbol.toPrimitive];if(n!==void 0){var r=n.call(e,t);if(re(r)!="object")return r;throw new TypeError("@@toPrimitive must return a primitive value.")}return(t==="string"?String:Number)(e)}function ee(e){return ir(e)||lr(e)||pt(e)||ar()}function ar(){throw new TypeError(`Invalid attempt to spread non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`)}function pt(e,t){if(e){if(typeof e=="string")return xe(e,t);var n={}.toString.call(e).slice(8,-1);return n==="Object"&&e.constructor&&(n=e.constructor.name),n==="Map"||n==="Set"?Array.from(e):n==="Arguments"||/^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)?xe(e,t):void 0}}function lr(e){if(typeof Symbol<"u"&&e[Symbol.iterator]!=null||e["@@iterator"]!=null)return Array.from(e)}function ir(e){if(Array.isArray(e))return xe(e)}function xe(e,t){(t==null||t>e.length)&&(t=e.length);for(var n=0,r=Array(t);n<t;n++)r[n]=e[n];return r}var ht={name:"TreeNode",hostName:"Tree",extends:Ae,emits:["node-toggle","node-click","checkbox-change"],props:{node:{type:null,default:null},expandedKeys:{type:null,default:null},loadingMode:{type:String,default:"mask"},selectionKeys:{type:null,default:null},selectionMode:{type:String,default:null},templates:{type:null,default:null},level:{type:Number,default:null},index:null},nodeTouched:!1,toggleClicked:!1,mounted:function(){this.setAllNodesTabIndexes()},methods:{toggle:function(){this.$emit("node-toggle",this.node),this.toggleClicked=!0},label:function(t){return typeof t.label=="function"?t.label():t.label},onChildNodeToggle:function(t){this.$emit("node-toggle",t)},getPTOptions:function(t){return this.ptm(t,{context:{node:this.node,index:this.index,expanded:this.expanded,selected:this.selected,checked:this.checked,partialChecked:this.partialChecked,leaf:this.leaf}})},onClick:function(t){if(this.toggleClicked||ge(t.target,'[data-pc-section="nodetogglebutton"]')||ge(t.target.parentElement,'[data-pc-section="nodetogglebutton"]')){this.toggleClicked=!1;return}this.isCheckboxSelectionMode()?this.node.selectable!=!1&&this.toggleCheckbox():this.$emit("node-click",{originalEvent:t,nodeTouched:this.nodeTouched,node:this.node}),this.nodeTouched=!1},onChildNodeClick:function(t){this.$emit("node-click",t)},onTouchEnd:function(){this.nodeTouched=!0},onKeyDown:function(t){if(this.isSameNode(t))switch(t.code){case"Tab":this.onTabKey(t);break;case"ArrowDown":this.onArrowDown(t);break;case"ArrowUp":this.onArrowUp(t);break;case"ArrowRight":this.onArrowRight(t);break;case"ArrowLeft":this.onArrowLeft(t);break;case"Enter":case"NumpadEnter":case"Space":this.onEnterKey(t);break}},onArrowDown:function(t){var n=t.target.getAttribute("data-pc-section")==="nodetogglebutton"?t.target.closest('[role="treeitem"]'):t.target,r=n.children[1];if(r)this.focusRowChange(n,r.children[0]);else if(n.nextElementSibling)this.focusRowChange(n,n.nextElementSibling);else{var a=this.findNextSiblingOfAncestor(n);a&&this.focusRowChange(n,a)}t.preventDefault()},onArrowUp:function(t){var n=t.target;if(n.previousElementSibling)this.focusRowChange(n,n.previousElementSibling,this.findLastVisibleDescendant(n.previousElementSibling));else{var r=this.getParentNodeElement(n);r&&this.focusRowChange(n,r)}t.preventDefault()},onArrowRight:function(t){var n=this;this.leaf||this.expanded||(t.currentTarget.tabIndex=-1,this.$emit("node-toggle",this.node),this.$nextTick(function(){n.onArrowDown(t)}))},onArrowLeft:function(t){var n=Ie(t.currentTarget,'[data-pc-section="nodetogglebutton"]');if(this.level===0&&!this.expanded)return!1;if(this.expanded&&!this.leaf)return n.click(),!1;var r=this.findBeforeClickableNode(t.currentTarget);r&&this.focusRowChange(t.currentTarget,r)},onEnterKey:function(t){this.setTabIndexForSelectionMode(t,this.nodeTouched),this.onClick(t),t.preventDefault()},onTabKey:function(){this.setAllNodesTabIndexes()},setAllNodesTabIndexes:function(){var t=Te(this.$refs.currentNode.closest('[data-pc-section="rootchildren"]'),'[role="treeitem"]'),n=ee(t).some(function(a){return a.getAttribute("aria-selected")==="true"||a.getAttribute("aria-checked")==="true"});if(ee(t).forEach(function(a){a.tabIndex=-1}),n){var r=ee(t).filter(function(a){return a.getAttribute("aria-selected")==="true"||a.getAttribute("aria-checked")==="true"});r[0].tabIndex=0;return}ee(t)[0].tabIndex=0},setTabIndexForSelectionMode:function(t,n){if(this.selectionMode!==null){var r=ee(Te(this.$refs.currentNode.parentElement,'[role="treeitem"]'));t.currentTarget.tabIndex=n===!1?-1:0,r.every(function(a){return a.tabIndex===-1})&&(r[0].tabIndex=0)}},focusRowChange:function(t,n,r){t.tabIndex="-1",n.tabIndex="0",this.focusNode(r||n)},findBeforeClickableNode:function(t){var n=t.closest("ul").closest("li");if(n){var r=Ie(n,"button");return r&&r.style.visibility!=="hidden"?n:this.findBeforeClickableNode(t.previousElementSibling)}return null},toggleCheckbox:function(){var t=this.selectionKeys?Ze({},this.selectionKeys):{},n=!this.checked;this.propagateDown(this.node,n,t),this.$emit("checkbox-change",{node:this.node,check:n,selectionKeys:t})},propagateDown:function(t,n,r){if(n&&t.selectable!=!1?r[t.key]={checked:!0,partialChecked:!1}:delete r[t.key],t.children&&t.children.length){var a=qe(t.children),o;try{for(a.s();!(o=a.n()).done;){var c=o.value;this.propagateDown(c,n,r)}}catch(g){a.e(g)}finally{a.f()}}},propagateUp:function(t){var n=t.check,r=Ze({},t.selectionKeys),a=0,o=!1,c=qe(this.node.children),g;try{for(c.s();!(g=c.n()).done;){var h=g.value;r[h.key]&&r[h.key].checked?a++:r[h.key]&&r[h.key].partialChecked&&(o=!0)}}catch(s){c.e(s)}finally{c.f()}n&&a===this.node.children.length?r[this.node.key]={checked:!0,partialChecked:!1}:(n||delete r[this.node.key],o||a>0&&a!==this.node.children.length?r[this.node.key]={checked:!1,partialChecked:!0}:delete r[this.node.key]),this.$emit("checkbox-change",{node:t.node,check:t.check,selectionKeys:r})},onChildCheckboxChange:function(t){this.$emit("checkbox-change",t)},findNextSiblingOfAncestor:function(t){var n=this.getParentNodeElement(t);return n?n.nextElementSibling?n.nextElementSibling:this.findNextSiblingOfAncestor(n):null},findLastVisibleDescendant:function(t){var n=t.children[1];if(n){var r=n.children[n.children.length-1];return this.findLastVisibleDescendant(r)}else return t},getParentNodeElement:function(t){var n=t.parentElement.parentElement;return ge(n,"role")==="treeitem"?n:null},focusNode:function(t){t.focus()},isCheckboxSelectionMode:function(){return this.selectionMode==="checkbox"},isSameNode:function(t){return t.currentTarget&&(t.currentTarget.isSameNode(t.target)||t.currentTarget.isSameNode(t.target.closest('[role="treeitem"]')))}},computed:{hasChildren:function(){return this.node.children&&this.node.children.length>0},expanded:function(){return this.expandedKeys&&this.expandedKeys[this.node.key]===!0},leaf:function(){return this.node.leaf===!1?!1:!(this.node.children&&this.node.children.length)},selectable:function(){return this.node.selectable===!1?!1:this.selectionMode!=null},selected:function(){return this.selectionMode&&this.selectionKeys?this.selectionKeys[this.node.key]===!0:!1},checkboxMode:function(){return this.selectionMode==="checkbox"&&this.node.selectable!==!1},checked:function(){return this.selectionKeys?this.selectionKeys[this.node.key]&&this.selectionKeys[this.node.key].checked:!1},partialChecked:function(){return this.selectionKeys?this.selectionKeys[this.node.key]&&this.selectionKeys[this.node.key].partialChecked:!1},ariaChecked:function(){return this.selectionMode==="single"||this.selectionMode==="multiple"?this.selected:void 0},ariaSelected:function(){return this.checkboxMode?this.checked:void 0}},components:{Checkbox:nn,ChevronDownIcon:ct,ChevronRightIcon:en,CheckIcon:Qt,MinusIcon:tn,SpinnerIcon:ut},directives:{ripple:tt}},sr=["aria-label","aria-selected","aria-expanded","aria-setsize","aria-posinset","aria-level","aria-checked","tabindex"],dr=["data-p-selected","data-p-selectable"],cr=["data-p-leaf"];function ur(e,t,n,r,a,o){var c=D("SpinnerIcon"),g=D("Checkbox"),h=D("TreeNode",!0),s=nt("ripple");return p(),w("li",m({ref:"currentNode",class:e.cx("node"),role:"treeitem","aria-label":o.label(n.node),"aria-selected":o.ariaSelected,"aria-expanded":o.expanded,"aria-setsize":n.node.children?n.node.children.length:0,"aria-posinset":n.index+1,"aria-level":n.level,"aria-checked":o.ariaChecked,tabindex:n.index===0?0:-1,onKeydown:t[4]||(t[4]=function(){return o.onKeyDown&&o.onKeyDown.apply(o,arguments)})},o.getPTOptions("node")),[d("div",m({class:e.cx("nodeContent"),onClick:t[2]||(t[2]=function(){return o.onClick&&o.onClick.apply(o,arguments)}),onTouchend:t[3]||(t[3]=function(){return o.onTouchEnd&&o.onTouchEnd.apply(o,arguments)}),style:n.node.style},o.getPTOptions("nodeContent"),{"data-p-selected":o.checkboxMode?o.checked:o.selected,"data-p-selectable":o.selectable}),[rt((p(),w("button",m({type:"button",class:e.cx("nodeToggleButton"),onClick:t[0]||(t[0]=function(){return o.toggle&&o.toggle.apply(o,arguments)}),tabindex:"-1","data-p-leaf":o.leaf},o.getPTOptions("nodeToggleButton")),[n.node.loading&&n.loadingMode==="icon"?(p(),w(F,{key:0},[n.templates.nodetoggleicon||n.templates.nodetogglericon?(p(),C(R(n.templates.nodetoggleicon||n.templates.nodetogglericon),{key:0,node:n.node,expanded:o.expanded,class:M(e.cx("nodeToggleIcon"))},null,8,["node","expanded","class"])):(p(),C(c,m({key:1,spin:"",class:e.cx("nodeToggleIcon")},o.getPTOptions("nodeToggleIcon")),null,16,["class"]))],64)):(p(),w(F,{key:1},[n.templates.nodetoggleicon||n.templates.togglericon?(p(),C(R(n.templates.nodetoggleicon||n.templates.togglericon),{key:0,node:n.node,expanded:o.expanded,class:M(e.cx("nodeToggleIcon"))},null,8,["node","expanded","class"])):o.expanded?(p(),C(R(n.node.expandedIcon?"span":"ChevronDownIcon"),m({key:1,class:e.cx("nodeToggleIcon")},o.getPTOptions("nodeToggleIcon")),null,16,["class"])):(p(),C(R(n.node.collapsedIcon?"span":"ChevronRightIcon"),m({key:2,class:e.cx("nodeToggleIcon")},o.getPTOptions("nodeToggleIcon")),null,16,["class"]))],64))],16,cr)),[[s]]),o.checkboxMode?(p(),C(g,{key:0,defaultValue:o.checked,binary:!0,indeterminate:o.partialChecked,class:M(e.cx("nodeCheckbox")),tabindex:-1,unstyled:e.unstyled,pt:o.getPTOptions("pcNodeCheckbox"),"data-p-partialchecked":o.partialChecked},{icon:I(function(k){return[n.templates.checkboxicon?(p(),C(R(n.templates.checkboxicon),{key:0,checked:k.checked,partialChecked:o.partialChecked,class:M(k.class)},null,8,["checked","partialChecked","class"])):S("",!0)]}),_:1},8,["defaultValue","indeterminate","class","unstyled","pt","data-p-partialchecked"])):S("",!0),n.templates.nodeicon?(p(),C(R(n.templates.nodeicon),m({key:1,node:n.node,class:[e.cx("nodeIcon")]},o.getPTOptions("nodeIcon")),null,16,["node","class"])):(p(),w("span",m({key:2,class:[e.cx("nodeIcon"),n.node.icon]},o.getPTOptions("nodeIcon")),null,16)),d("span",m({class:e.cx("nodeLabel")},o.getPTOptions("nodeLabel"),{onKeydown:t[1]||(t[1]=ot(function(){},["stop"]))}),[n.templates[n.node.type]||n.templates.default?(p(),C(R(n.templates[n.node.type]||n.templates.default),{key:0,node:n.node,expanded:o.expanded,selected:o.checkboxMode?o.checked:o.selected},null,8,["node","expanded","selected"])):(p(),w(F,{key:1},[K(A(o.label(n.node)),1)],64))],16)],16,dr),o.hasChildren&&o.expanded?(p(),w("ul",m({key:0,class:e.cx("nodeChildren"),role:"group"},e.ptm("nodeChildren")),[(p(!0),w(F,null,Ke(n.node.children,function(k){return p(),C(h,{key:k.key,node:k,templates:n.templates,level:n.level+1,loadingMode:n.loadingMode,expandedKeys:n.expandedKeys,onNodeToggle:o.onChildNodeToggle,onNodeClick:o.onChildNodeClick,selectionMode:n.selectionMode,selectionKeys:n.selectionKeys,onCheckboxChange:o.propagateUp,unstyled:e.unstyled,pt:e.pt},null,8,["node","templates","level","loadingMode","expandedKeys","onNodeToggle","onNodeClick","selectionMode","selectionKeys","onCheckboxChange","unstyled","pt"])}),128))],16)):S("",!0)],16,sr)}ht.render=ur;function oe(e){"@babel/helpers - typeof";return oe=typeof Symbol=="function"&&typeof Symbol.iterator=="symbol"?function(t){return typeof t}:function(t){return t&&typeof Symbol=="function"&&t.constructor===Symbol&&t!==Symbol.prototype?"symbol":typeof t},oe(e)}function ke(e,t){var n=typeof Symbol<"u"&&e[Symbol.iterator]||e["@@iterator"];if(!n){if(Array.isArray(e)||(n=yt(e))||t){n&&(e=n);var r=0,a=function(){};return{s:a,n:function(){return r>=e.length?{done:!0}:{done:!1,value:e[r++]}},e:function(s){throw s},f:a}}throw new TypeError(`Invalid attempt to iterate non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`)}var o,c=!0,g=!1;return{s:function(){n=n.call(e)},n:function(){var s=n.next();return c=s.done,s},e:function(s){g=!0,o=s},f:function(){try{c||n.return==null||n.return()}finally{if(g)throw o}}}}function fr(e){return yr(e)||hr(e)||yt(e)||pr()}function pr(){throw new TypeError(`Invalid attempt to spread non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`)}function yt(e,t){if(e){if(typeof e=="string")return Oe(e,t);var n={}.toString.call(e).slice(8,-1);return n==="Object"&&e.constructor&&(n=e.constructor.name),n==="Map"||n==="Set"?Array.from(e):n==="Arguments"||/^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)?Oe(e,t):void 0}}function hr(e){if(typeof Symbol<"u"&&e[Symbol.iterator]!=null||e["@@iterator"]!=null)return Array.from(e)}function yr(e){if(Array.isArray(e))return Oe(e)}function Oe(e,t){(t==null||t>e.length)&&(t=e.length);for(var n=0,r=Array(t);n<t;n++)r[n]=e[n];return r}function Ye(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter(function(a){return Object.getOwnPropertyDescriptor(e,a).enumerable})),n.push.apply(n,r)}return n}function W(e){for(var t=1;t<arguments.length;t++){var n=arguments[t]!=null?arguments[t]:{};t%2?Ye(Object(n),!0).forEach(function(r){mr(e,r,n[r])}):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):Ye(Object(n)).forEach(function(r){Object.defineProperty(e,r,Object.getOwnPropertyDescriptor(n,r))})}return e}function mr(e,t,n){return(t=gr(t))in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function gr(e){var t=br(e,"string");return oe(t)=="symbol"?t:t+""}function br(e,t){if(oe(e)!="object"||!e)return e;var n=e[Symbol.toPrimitive];if(n!==void 0){var r=n.call(e,t);if(oe(r)!="object")return r;throw new TypeError("@@toPrimitive must return a primitive value.")}return(t==="string"?String:Number)(e)}var mt={name:"Tree",extends:tr,inheritAttrs:!1,emits:["node-expand","node-collapse","update:expandedKeys","update:selectionKeys","node-select","node-unselect","filter"],data:function(){return{d_expandedKeys:this.expandedKeys||{},filterValue:null}},watch:{expandedKeys:function(t){this.d_expandedKeys=t}},methods:{onNodeToggle:function(t){var n=t.key;this.d_expandedKeys[n]?(delete this.d_expandedKeys[n],this.$emit("node-collapse",t)):(this.d_expandedKeys[n]=!0,this.$emit("node-expand",t)),this.d_expandedKeys=W({},this.d_expandedKeys),this.$emit("update:expandedKeys",this.d_expandedKeys)},onNodeClick:function(t){if(this.selectionMode!=null&&t.node.selectable!==!1){var n=t.nodeTouched?!1:this.metaKeySelection,r=n?this.handleSelectionWithMetaKey(t):this.handleSelectionWithoutMetaKey(t);this.$emit("update:selectionKeys",r)}},onCheckboxChange:function(t){this.$emit("update:selectionKeys",t.selectionKeys),t.check?this.$emit("node-select",t.node):this.$emit("node-unselect",t.node)},handleSelectionWithMetaKey:function(t){var n=t.originalEvent,r=t.node,a=n.metaKey||n.ctrlKey,o=this.isNodeSelected(r),c;return o&&a?(this.isSingleSelectionMode()?c={}:(c=W({},this.selectionKeys),delete c[r.key]),this.$emit("node-unselect",r)):(this.isSingleSelectionMode()?c={}:this.isMultipleSelectionMode()&&(c=a?this.selectionKeys?W({},this.selectionKeys):{}:{}),c[r.key]=!0,this.$emit("node-select",r)),c},handleSelectionWithoutMetaKey:function(t){var n=t.node,r=this.isNodeSelected(n),a;return this.isSingleSelectionMode()?r?(a={},this.$emit("node-unselect",n)):(a={},a[n.key]=!0,this.$emit("node-select",n)):r?(a=W({},this.selectionKeys),delete a[n.key],this.$emit("node-unselect",n)):(a=this.selectionKeys?W({},this.selectionKeys):{},a[n.key]=!0,this.$emit("node-select",n)),a},isSingleSelectionMode:function(){return this.selectionMode==="single"},isMultipleSelectionMode:function(){return this.selectionMode==="multiple"},isNodeSelected:function(t){return this.selectionMode&&this.selectionKeys?this.selectionKeys[t.key]===!0:!1},isChecked:function(t){return this.selectionKeys?this.selectionKeys[t.key]&&this.selectionKeys[t.key].checked:!1},isNodeLeaf:function(t){return t.leaf===!1?!1:!(t.children&&t.children.length)},onFilterKeyup:function(t){(t.code==="Enter"||t.code==="NumpadEnter")&&t.preventDefault(),this.$emit("filter",{originalEvent:t,value:t.target.value})},findFilteredNodes:function(t,n){if(t){var r=!1;if(t.children){var a=fr(t.children);t.children=[];var o=ke(a),c;try{for(o.s();!(c=o.n()).done;){var g=c.value,h=W({},g);this.isFilterMatched(h,n)&&(r=!0,t.children.push(h))}}catch(s){o.e(s)}finally{o.f()}}if(r)return!0}},isFilterMatched:function(t,n){var r=n.searchFields,a=n.filterText,o=n.strict,c=!1,g=ke(r),h;try{for(g.s();!(h=g.n()).done;){var s=h.value,k=String(Ot(t,s)).toLocaleLowerCase(this.filterLocale);k.indexOf(a)>-1&&(c=!0)}}catch(N){g.e(N)}finally{g.f()}return(!c||o&&!this.isNodeLeaf(t))&&(c=this.findFilteredNodes(t,{searchFields:r,filterText:a,strict:o})||c),c}},computed:{filteredValue:function(){var t=[],n=xt(this.filterBy)?[this.filterBy]:this.filterBy.split(","),r=this.filterValue.trim().toLocaleLowerCase(this.filterLocale),a=this.filterMode==="strict",o=ke(this.value),c;try{for(o.s();!(c=o.n()).done;){var g=c.value,h=W({},g),s={searchFields:n,filterText:r,strict:a};(a&&(this.findFilteredNodes(h,s)||this.isFilterMatched(h,s))||!a&&(this.isFilterMatched(h,s)||this.findFilteredNodes(h,s)))&&t.push(h)}}catch(k){o.e(k)}finally{o.f()}return t},valueToRender:function(){return this.filterValue&&this.filterValue.trim().length>0?this.filteredValue:this.value},containerDataP:function(){return ye({loading:this.loading,scrollable:this.scrollHeight==="flex"})},wrapperDataP:function(){return ye({scrollable:this.scrollHeight==="flex"})}},components:{TreeNode:ht,InputText:st,InputIcon:Jt,IconField:Xt,SearchIcon:Gt,SpinnerIcon:ut}};function ae(e){"@babel/helpers - typeof";return ae=typeof Symbol=="function"&&typeof Symbol.iterator=="symbol"?function(t){return typeof t}:function(t){return t&&typeof Symbol=="function"&&t.constructor===Symbol&&t!==Symbol.prototype?"symbol":typeof t},ae(e)}function Ge(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter(function(a){return Object.getOwnPropertyDescriptor(e,a).enumerable})),n.push.apply(n,r)}return n}function Xe(e){for(var t=1;t<arguments.length;t++){var n=arguments[t]!=null?arguments[t]:{};t%2?Ge(Object(n),!0).forEach(function(r){vr(e,r,n[r])}):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):Ge(Object(n)).forEach(function(r){Object.defineProperty(e,r,Object.getOwnPropertyDescriptor(n,r))})}return e}function vr(e,t,n){return(t=wr(t))in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function wr(e){var t=kr(e,"string");return ae(t)=="symbol"?t:t+""}function kr(e,t){if(ae(e)!="object"||!e)return e;var n=e[Symbol.toPrimitive];if(n!==void 0){var r=n.call(e,t);if(ae(r)!="object")return r;throw new TypeError("@@toPrimitive must return a primitive value.")}return(t==="string"?String:Number)(e)}var Sr=["data-p"],Cr=["data-p"],Ir=["aria-labelledby","aria-label"];function Tr(e,t,n,r,a,o){var c=D("SpinnerIcon"),g=D("InputText"),h=D("SearchIcon"),s=D("InputIcon"),k=D("IconField"),N=D("TreeNode");return p(),w("div",m({class:e.cx("root"),"data-p":o.containerDataP},e.ptmi("root")),[e.loading&&e.loadingMode==="mask"?(p(),w("div",m({key:0,class:e.cx("mask")},e.ptm("mask")),[O(e.$slots,"loadingicon",{class:M(e.cx("loadingIcon"))},function(){return[e.loadingIcon?(p(),w("i",m({key:0,class:[e.cx("loadingIcon"),"pi-spin",e.loadingIcon]},e.ptm("loadingIcon")),null,16)):(p(),C(c,m({key:1,spin:"",class:e.cx("loadingIcon")},e.ptm("loadingIcon")),null,16,["class"]))]})],16)):S("",!0),e.filter?(p(),C(k,{key:1,unstyled:e.unstyled,pt:Xe(Xe({},e.ptm("pcFilter")),e.ptm("pcFilterContainer")),class:M(e.cx("pcFilterContainer"))},{default:I(function(){return[T(g,{modelValue:a.filterValue,"onUpdate:modelValue":t[0]||(t[0]=function(z){return a.filterValue=z}),autocomplete:"off",class:M(e.cx("pcFilterInput")),placeholder:e.filterPlaceholder,unstyled:e.unstyled,onKeyup:o.onFilterKeyup,pt:e.ptm("pcFilterInput")},null,8,["modelValue","class","placeholder","unstyled","onKeyup","pt"]),T(s,{unstyled:e.unstyled,pt:e.ptm("pcFilterIconContainer")},{default:I(function(){return[O(e.$slots,e.$slots.filtericon?"filtericon":"searchicon",{class:M(e.cx("filterIcon"))},function(){return[T(h,m({class:e.cx("filterIcon")},e.ptm("filterIcon")),null,16,["class"])]})]}),_:3},8,["unstyled","pt"])]}),_:3},8,["unstyled","pt","class"])):S("",!0),d("div",m({class:e.cx("wrapper"),style:{maxHeight:e.scrollHeight},"data-p":o.wrapperDataP},e.ptm("wrapper")),[O(e.$slots,"header",{value:e.value,expandedKeys:e.expandedKeys,selectionKeys:e.selectionKeys}),d("ul",m({class:e.cx("rootChildren"),role:"tree","aria-labelledby":e.ariaLabelledby,"aria-label":e.ariaLabel},e.ptm("rootChildren")),[(p(!0),w(F,null,Ke(o.valueToRender,function(z,l){return p(),C(N,{key:z.key,node:z,templates:e.$slots,level:e.level+1,index:l,expandedKeys:a.d_expandedKeys,onNodeToggle:o.onNodeToggle,onNodeClick:o.onNodeClick,selectionMode:e.selectionMode,selectionKeys:e.selectionKeys,onCheckboxChange:o.onCheckboxChange,loadingMode:e.loadingMode,unstyled:e.unstyled,pt:e.pt},null,8,["node","templates","level","index","expandedKeys","onNodeToggle","onNodeClick","selectionMode","selectionKeys","onCheckboxChange","loadingMode","unstyled","pt"])}),128))],16,Ir),O(e.$slots,"footer",{value:e.value,expandedKeys:e.expandedKeys,selectionKeys:e.selectionKeys})],16,Cr)],16,Sr)}mt.render=Tr;var xr=`
    .p-treeselect {
        display: inline-flex;
        cursor: pointer;
        position: relative;
        user-select: none;
        background: dt('treeselect.background');
        border: 1px solid dt('treeselect.border.color');
        transition:
            background dt('treeselect.transition.duration'),
            color dt('treeselect.transition.duration'),
            border-color dt('treeselect.transition.duration'),
            outline-color dt('treeselect.transition.duration'),
            box-shadow dt('treeselect.transition.duration');
        border-radius: dt('treeselect.border.radius');
        outline-color: transparent;
        box-shadow: dt('treeselect.shadow');
    }

    .p-treeselect:not(.p-disabled):hover {
        border-color: dt('treeselect.hover.border.color');
    }

    .p-treeselect:not(.p-disabled).p-focus {
        border-color: dt('treeselect.focus.border.color');
        box-shadow: dt('treeselect.focus.ring.shadow');
        outline: dt('treeselect.focus.ring.width') dt('treeselect.focus.ring.style') dt('treeselect.focus.ring.color');
        outline-offset: dt('treeselect.focus.ring.offset');
    }

    .p-treeselect.p-variant-filled {
        background: dt('treeselect.filled.background');
    }

    .p-treeselect.p-variant-filled:not(.p-disabled):hover {
        background: dt('treeselect.filled.hover.background');
    }

    .p-treeselect.p-variant-filled.p-focus {
        background: dt('treeselect.filled.focus.background');
    }

    .p-treeselect.p-invalid {
        border-color: dt('treeselect.invalid.border.color');
    }

    .p-treeselect.p-disabled {
        opacity: 1;
        background: dt('treeselect.disabled.background');
    }

    .p-treeselect-clear-icon {
        position: absolute;
        top: 50%;
        margin-top: -0.5rem;
        color: dt('treeselect.clear.icon.color');
        inset-inline-end: dt('treeselect.dropdown.width');
    }

    .p-treeselect-dropdown {
        display: flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 0;
        background: transparent;
        color: dt('treeselect.dropdown.color');
        width: dt('treeselect.dropdown.width');
        border-start-end-radius: dt('border.radius.md');
        border-end-end-radius: dt('border.radius.md');
    }

    .p-treeselect-label-container {
        overflow: hidden;
        flex: 1 1 auto;
        cursor: pointer;
    }

    .p-treeselect-label {
        display: flex;
        align-items: center;
        gap: calc(dt('treeselect.padding.y') / 2);
        white-space: nowrap;
        cursor: pointer;
        overflow: hidden;
        text-overflow: ellipsis;
        padding: dt('treeselect.padding.y') dt('treeselect.padding.x');
        color: dt('treeselect.color');
    }

    .p-treeselect-label.p-placeholder {
        color: dt('treeselect.placeholder.color');
    }

    .p-treeselect.p-invalid .p-treeselect-label.p-placeholder {
        color: dt('treeselect.invalid.placeholder.color');
    }

    .p-treeselect.p-disabled .p-treeselect-label {
        color: dt('treeselect.disabled.color');
    }

    .p-treeselect-label-empty {
        overflow: hidden;
        visibility: hidden;
    }

    .p-treeselect-overlay {
        position: absolute;
        top: 0;
        left: 0;
        background: dt('treeselect.overlay.background');
        color: dt('treeselect.overlay.color');
        border: 1px solid dt('treeselect.overlay.border.color');
        border-radius: dt('treeselect.overlay.border.radius');
        box-shadow: dt('treeselect.overlay.shadow');
        overflow: hidden;
        min-width: 100%;
    }

    .p-treeselect-tree-container {
        overflow: auto;
    }

    .p-treeselect-empty-message {
        padding: dt('treeselect.empty.message.padding');
        background: transparent;
    }

    .p-treeselect-fluid {
        display: flex;
    }

    .p-treeselect-overlay .p-tree {
        padding: dt('treeselect.tree.padding');
    }

    .p-treeselect-overlay .p-tree-loading {
        min-height: 3rem;
    }

    .p-treeselect-label .p-chip {
        padding-block-start: calc(dt('treeselect.padding.y') / 2);
        padding-block-end: calc(dt('treeselect.padding.y') / 2);
        border-radius: dt('treeselect.chip.border.radius');
    }

    .p-treeselect-label:has(.p-chip) {
        padding: calc(dt('treeselect.padding.y') / 2) calc(dt('treeselect.padding.x') / 2);
    }

    .p-treeselect-sm .p-treeselect-label {
        font-size: dt('treeselect.sm.font.size');
        padding-block: dt('treeselect.sm.padding.y');
        padding-inline: dt('treeselect.sm.padding.x');
    }

    .p-treeselect-sm .p-treeselect-dropdown .p-icon {
        font-size: dt('treeselect.sm.font.size');
        width: dt('treeselect.sm.font.size');
        height: dt('treeselect.sm.font.size');
    }

    .p-treeselect-lg .p-treeselect-label {
        font-size: dt('treeselect.lg.font.size');
        padding-block: dt('treeselect.lg.padding.y');
        padding-inline: dt('treeselect.lg.padding.x');
    }

    .p-treeselect-lg .p-treeselect-dropdown .p-icon {
        font-size: dt('treeselect.lg.font.size');
        width: dt('treeselect.lg.font.size');
        height: dt('treeselect.lg.font.size');
    }
`,Or={root:function(t){var n=t.props;return{position:n.appendTo==="self"?"relative":void 0}}},Lr={root:function(t){var n=t.instance,r=t.props;return["p-treeselect p-component p-inputwrapper",{"p-treeselect-display-chip":r.display==="chip","p-disabled":r.disabled,"p-invalid":n.$invalid,"p-focus":n.focused,"p-variant-filled":n.$variant==="filled","p-inputwrapper-filled":n.$filled,"p-inputwrapper-focus":n.focused||n.overlayVisible,"p-treeselect-open":n.overlayVisible,"p-treeselect-fluid":n.$fluid,"p-treeselect-sm p-inputfield-sm":r.size==="small","p-treeselect-lg p-inputfield-lg":r.size==="large"}]},labelContainer:"p-treeselect-label-container",label:function(t){var n=t.instance,r=t.props;return["p-treeselect-label",{"p-placeholder":n.label===r.placeholder,"p-treeselect-label-empty":!r.placeholder&&n.emptyValue}]},clearIcon:"p-treeselect-clear-icon",chip:"p-treeselect-chip-item",pcChip:"p-treeselect-chip",dropdown:"p-treeselect-dropdown",dropdownIcon:"p-treeselect-dropdown-icon",panel:"p-treeselect-overlay p-component",treeContainer:"p-treeselect-tree-container",emptyMessage:"p-treeselect-empty-message"},Ar=me.extend({name:"treeselect",style:xr,classes:Lr,inlineStyles:Or}),Kr={name:"BaseTreeSelect",extends:it,props:{options:Array,scrollHeight:{type:String,default:"20rem"},placeholder:{type:String,default:null},tabindex:{type:Number,default:null},selectionMode:{type:String,default:"single"},selectedItemsLabel:{type:String,default:null},maxSelectedLabels:{type:Number,default:null},appendTo:{type:[String,Object],default:"body"},emptyMessage:{type:String,default:null},display:{type:String,default:"comma"},metaKeySelection:{type:Boolean,default:!1},loading:{type:Boolean,default:!1},loadingIcon:{type:String,default:void 0},loadingMode:{type:String,default:"mask"},showClear:{type:Boolean,default:!1},clearIcon:{type:String,default:void 0},filter:{type:Boolean,default:!1},filterBy:{type:[String,Function],default:"label"},filterMode:{type:String,default:"lenient"},filterPlaceholder:{type:String,default:null},filterLocale:{type:String,default:void 0},inputId:{type:String,default:null},inputClass:{type:[String,Object],default:null},inputStyle:{type:Object,default:null},inputProps:{type:null,default:null},panelClass:{type:[String,Object],default:null},panelProps:{type:null,default:null},ariaLabelledby:{type:String,default:null},ariaLabel:{type:String,default:null},expandedKeys:{type:null,default:null}},style:Ar,provide:function(){return{$pcTreeSelect:this,$parentInstance:this}}};function le(e){"@babel/helpers - typeof";return le=typeof Symbol=="function"&&typeof Symbol.iterator=="symbol"?function(t){return typeof t}:function(t){return t&&typeof Symbol=="function"&&t.constructor===Symbol&&t!==Symbol.prototype?"symbol":typeof t},le(e)}function Se(e,t){var n=typeof Symbol<"u"&&e[Symbol.iterator]||e["@@iterator"];if(!n){if(Array.isArray(e)||(n=gt(e))||t){n&&(e=n);var r=0,a=function(){};return{s:a,n:function(){return r>=e.length?{done:!0}:{done:!1,value:e[r++]}},e:function(s){throw s},f:a}}throw new TypeError(`Invalid attempt to iterate non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`)}var o,c=!0,g=!1;return{s:function(){n=n.call(e)},n:function(){var s=n.next();return c=s.done,s},e:function(s){g=!0,o=s},f:function(){try{c||n.return==null||n.return()}finally{if(g)throw o}}}}function Je(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter(function(a){return Object.getOwnPropertyDescriptor(e,a).enumerable})),n.push.apply(n,r)}return n}function Qe(e){for(var t=1;t<arguments.length;t++){var n=arguments[t]!=null?arguments[t]:{};t%2?Je(Object(n),!0).forEach(function(r){Dr(e,r,n[r])}):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):Je(Object(n)).forEach(function(r){Object.defineProperty(e,r,Object.getOwnPropertyDescriptor(n,r))})}return e}function Dr(e,t,n){return(t=Mr(t))in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function Mr(e){var t=Pr(e,"string");return le(t)=="symbol"?t:t+""}function Pr(e,t){if(le(e)!="object"||!e)return e;var n=e[Symbol.toPrimitive];if(n!==void 0){var r=n.call(e,t);if(le(r)!="object")return r;throw new TypeError("@@toPrimitive must return a primitive value.")}return(t==="string"?String:Number)(e)}function Fr(e){return jr(e)||$r(e)||gt(e)||Nr()}function Nr(){throw new TypeError(`Invalid attempt to spread non-iterable instance.
In order to be iterable, non-array objects must have a [Symbol.iterator]() method.`)}function gt(e,t){if(e){if(typeof e=="string")return Le(e,t);var n={}.toString.call(e).slice(8,-1);return n==="Object"&&e.constructor&&(n=e.constructor.name),n==="Map"||n==="Set"?Array.from(e):n==="Arguments"||/^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)?Le(e,t):void 0}}function $r(e){if(typeof Symbol<"u"&&e[Symbol.iterator]!=null||e["@@iterator"]!=null)return Array.from(e)}function jr(e){if(Array.isArray(e))return Le(e)}function Le(e,t){(t==null||t>e.length)&&(t=e.length);for(var n=0,r=Array(t);n<t;n++)r[n]=e[n];return r}var zr={name:"TreeSelect",extends:Kr,inheritAttrs:!1,emits:["before-show","before-hide","change","show","hide","node-select","node-unselect","node-expand","node-collapse","focus","blur","update:expandedKeys"],inject:{$pcFluid:{default:null}},data:function(){return{focused:!1,overlayVisible:!1,d_expandedKeys:this.expandedKeys||{}}},watch:{modelValue:{handler:function(){this.selfChange||this.updateTreeState(),this.selfChange=!1},immediate:!0},options:function(){this.updateTreeState()},expandedKeys:function(t){this.d_expandedKeys=t}},outsideClickListener:null,resizeListener:null,scrollHandler:null,overlay:null,selfChange:!1,selfClick:!1,beforeUnmount:function(){this.unbindOutsideClickListener(),this.unbindResizeListener(),this.scrollHandler&&(this.scrollHandler.destroy(),this.scrollHandler=null),this.overlay&&(Q.clear(this.overlay),this.overlay=null)},mounted:function(){this.updateTreeState()},methods:{show:function(){this.$emit("before-show"),this.overlayVisible=!0},hide:function(){this.$emit("before-hide"),this.overlayVisible=!1,this.$refs.focusInput.focus()},onFocus:function(t){this.focused=!0,this.$emit("focus",t)},onBlur:function(t){var n,r;this.focused=!1,this.$emit("blur",t),(n=(r=this.formField).onBlur)===null||n===void 0||n.call(r)},onClick:function(t){this.disabled||t.target.tagName==="INPUT"||t.target.getAttribute("data-pc-section")==="clearicon"||t.target.closest('[data-pc-section="clearicon"]')||(!this.overlay||!this.overlay.contains(t.target))&&(this.overlayVisible?this.hide():this.show(),X(this.$refs.focusInput))},onClearClick:function(){this.onSelectionChange(null)},onSelectionChange:function(t){this.selfChange=!0,this.writeValue(t),this.$emit("change",t)},onNodeSelect:function(t){this.$emit("node-select",t),this.selectionMode==="single"&&this.hide()},onNodeUnselect:function(t){this.$emit("node-unselect",t)},onNodeToggle:function(t){this.d_expandedKeys=t,this.$emit("update:expandedKeys",this.d_expandedKeys)},getSelectedItemsLabel:function(){var t=/{(.*?)}/,n=this.selectedItemsLabel||this.$primevue.config.locale.selectionMessage;return t.test(n)?n.replace(n.match(t)[0],Object.keys(this.d_value).length+""):n},onFirstHiddenFocus:function(t){var n=t.relatedTarget===this.$refs.focusInput?jt(this.overlay,':not([data-p-hidden-focusable="true"])'):this.$refs.focusInput;X(n)},onLastHiddenFocus:function(t){var n=t.relatedTarget===this.$refs.focusInput?$t(this.overlay,':not([data-p-hidden-focusable="true"])'):this.$refs.focusInput;X(n)},onKeyDown:function(t){switch(t.code){case"ArrowDown":this.onArrowDownKey(t);break;case"Space":case"Enter":case"NumpadEnter":this.onEnterKey(t);break;case"Escape":this.onEscapeKey(t);break;case"Tab":this.onTabKey(t);break}},onArrowDownKey:function(t){var n=this;this.overlayVisible||(this.show(),this.$nextTick(function(){var r=Te(n.$refs.tree.$el,'[data-pc-section="treeitem"]'),a=Fr(r).find(function(o){return o.getAttribute("tabindex")==="0"});X(a)}),t.preventDefault())},onEnterKey:function(t){this.overlayVisible?this.hide():this.onArrowDownKey(t),t.preventDefault()},onEscapeKey:function(t){this.overlayVisible&&(this.hide(),t.preventDefault())},onTabKey:function(t){var n=arguments.length>1&&arguments[1]!==void 0?arguments[1]:!1;n||this.overlayVisible&&this.hasFocusableElements()&&(X(this.$refs.firstHiddenFocusableElementOnOverlay),t.preventDefault())},hasFocusableElements:function(){return Ue(this.overlay,':not([data-p-hidden-focusable="true"])').length>0},onOverlayEnter:function(t){Q.set("overlay",t,this.$primevue.config.zIndex.overlay),Nt(t,{position:"absolute",top:"0"}),this.alignOverlay(),this.focus(),this.$attrSelector&&t.setAttribute(this.$attrSelector,"")},onOverlayAfterEnter:function(){this.bindOutsideClickListener(),this.bindScrollListener(),this.bindResizeListener(),this.scrollValueInView(),this.$emit("show")},onOverlayLeave:function(){this.unbindOutsideClickListener(),this.unbindScrollListener(),this.unbindResizeListener(),this.$emit("hide"),this.overlay=null},onOverlayAfterLeave:function(t){Q.clear(t)},focus:function(){var t=Ue(this.overlay);t&&t.length>0&&t[0].focus()},alignOverlay:function(){this.appendTo==="self"?Mt(this.overlay,this.$el):(this.overlay.style.minWidth=Pt(this.$el)+"px",Ft(this.overlay,this.$el))},bindOutsideClickListener:function(){var t=this;this.outsideClickListener||(this.outsideClickListener=function(n){t.overlayVisible&&!t.selfClick&&t.isOutsideClicked(n)&&t.hide(),t.selfClick=!1},document.addEventListener("click",this.outsideClickListener,!0))},unbindOutsideClickListener:function(){this.outsideClickListener&&(document.removeEventListener("click",this.outsideClickListener,!0),this.outsideClickListener=null)},bindScrollListener:function(){var t=this;this.scrollHandler||(this.scrollHandler=new Dt(this.$refs.container,function(){t.overlayVisible&&t.hide()})),this.scrollHandler.bindScrollListener()},unbindScrollListener:function(){this.scrollHandler&&this.scrollHandler.unbindScrollListener()},bindResizeListener:function(){var t=this;this.resizeListener||(this.resizeListener=function(){t.overlayVisible&&!Kt()&&t.hide()},window.addEventListener("resize",this.resizeListener))},unbindResizeListener:function(){this.resizeListener&&(window.removeEventListener("resize",this.resizeListener),this.resizeListener=null)},isOutsideClicked:function(t){return!(this.$el.isSameNode(t.target)||this.$el.contains(t.target)||this.overlay&&this.overlay.contains(t.target))},overlayRef:function(t){this.overlay=t},onOverlayClick:function(t){At.emit("overlay-click",{originalEvent:t,target:this.$el}),this.selfClick=!0},onOverlayKeydown:function(t){t.code==="Escape"&&this.hide()},fillNodeMap:function(t,n){var r,a=this;n[t.key]=t,(r=t.children)!==null&&r!==void 0&&r.length&&t.children.forEach(function(o){return a.fillNodeMap(o,n)})},isSelected:function(t,n){return this.selectionMode==="checkbox"?n[t.key]&&n[t.key].checked:n[t.key]},updateTreeState:function(){var t=Qe({},this.d_value);t&&this.options&&this.updateTreeBranchState(null,null,t)},updateTreeBranchState:function(t,n,r){if(t){if(this.isSelected(t,r)&&(this.expandPath(n),delete r[t.key]),Object.keys(r).length&&t.children){var a=Se(t.children),o;try{for(a.s();!(o=a.n()).done;){var c=o.value;n.push(t.key),this.updateTreeBranchState(c,n,r)}}catch(k){a.e(k)}finally{a.f()}}}else{var g=Se(this.options),h;try{for(g.s();!(h=g.n()).done;){var s=h.value;this.updateTreeBranchState(s,[],r)}}catch(k){g.e(k)}finally{g.f()}}},expandPath:function(t){if(t.length>0){var n=Se(t),r;try{for(n.s();!(r=n.n()).done;){var a=r.value;this.d_expandedKeys[a]=!0}}catch(o){n.e(o)}finally{n.f()}this.d_expandedKeys=Qe({},this.d_expandedKeys),this.$emit("update:expandedKeys",this.d_expandedKeys)}},scrollValueInView:function(){if(this.overlay){var t=Ie(this.overlay,'[data-p-selected="true"]');t&&t.scrollIntoView({block:"nearest",inline:"start"})}}},computed:{nodeMap:function(){var t,n=this,r={};return(t=this.options)===null||t===void 0||t.forEach(function(a){return n.fillNodeMap(a,r)}),r},selectedNodes:function(){var t=this,n=[];return this.d_value&&this.options&&Object.keys(this.d_value).forEach(function(r){var a=t.nodeMap[r];t.isSelected(a,t.d_value)&&n.push(a)}),n},label:function(){var t=this.selectedNodes,n;return t.length?be(this.maxSelectedLabels)&&t.length>this.maxSelectedLabels?n=this.getSelectedItemsLabel():n=t.map(function(r){return r.label}).join(", "):n=this.placeholder,n},chipSelectedItems:function(){return be(this.maxSelectedLabels)&&this.d_value&&Object.keys(this.d_value).length>this.maxSelectedLabels},emptyMessageText:function(){return this.emptyMessage||this.$primevue.config.locale.emptyMessage},emptyValue:function(){return!this.$filled},emptyOptions:function(){return!this.options||this.options.length===0},listId:function(){return this.$id+"_list"},hasFluid:function(){return Lt(this.fluid)?!!this.$pcFluid:this.fluid},isClearIconVisible:function(){return this.showClear&&this.d_value!=null&&be(this.options)}},components:{TSTree:mt,Chip:Ht,Portal:at,ChevronDownIcon:ct,TimesIcon:dt},directives:{ripple:tt}};function ie(e){"@babel/helpers - typeof";return ie=typeof Symbol=="function"&&typeof Symbol.iterator=="symbol"?function(t){return typeof t}:function(t){return t&&typeof Symbol=="function"&&t.constructor===Symbol&&t!==Symbol.prototype?"symbol":typeof t},ie(e)}function _e(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter(function(a){return Object.getOwnPropertyDescriptor(e,a).enumerable})),n.push.apply(n,r)}return n}function he(e){for(var t=1;t<arguments.length;t++){var n=arguments[t]!=null?arguments[t]:{};t%2?_e(Object(n),!0).forEach(function(r){Vr(e,r,n[r])}):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):_e(Object(n)).forEach(function(r){Object.defineProperty(e,r,Object.getOwnPropertyDescriptor(n,r))})}return e}function Vr(e,t,n){return(t=Br(t))in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function Br(e){var t=Rr(e,"string");return ie(t)=="symbol"?t:t+""}function Rr(e,t){if(ie(e)!="object"||!e)return e;var n=e[Symbol.toPrimitive];if(n!==void 0){var r=n.call(e,t);if(ie(r)!="object")return r;throw new TypeError("@@toPrimitive must return a primitive value.")}return(t==="string"?String:Number)(e)}var Er=["id","disabled","tabindex","aria-labelledby","aria-label","aria-expanded","aria-controls"],Ur={key:0},Hr=["aria-expanded"];function qr(e,t,n,r,a,o){var c=D("Chip"),g=D("TSTree"),h=D("Portal");return p(),w("div",m({ref:"container",class:e.cx("root"),style:e.sx("root"),onClick:t[10]||(t[10]=function(){return o.onClick&&o.onClick.apply(o,arguments)})},e.ptmi("root")),[d("div",m({class:"p-hidden-accessible"},e.ptm("hiddenInputContainer"),{"data-p-hidden-accessible":!0}),[d("input",m({ref:"focusInput",id:e.inputId,type:"text",role:"combobox",class:e.inputClass,style:e.inputStyle,readonly:"",disabled:e.disabled,tabindex:e.disabled?-1:e.tabindex,"aria-labelledby":e.ariaLabelledby,"aria-label":e.ariaLabel,"aria-haspopup":"tree","aria-expanded":a.overlayVisible,"aria-controls":o.listId,onFocus:t[0]||(t[0]=function(s){return o.onFocus(s)}),onBlur:t[1]||(t[1]=function(s){return o.onBlur(s)}),onKeydown:t[2]||(t[2]=function(s){return o.onKeyDown(s)})},he(he({},e.inputProps),e.ptm("hiddenInput"))),null,16,Er)],16),d("div",m({class:e.cx("labelContainer")},e.ptm("labelContainer")),[d("div",m({class:e.cx("label")},e.ptm("label")),[O(e.$slots,"value",{value:o.selectedNodes,placeholder:e.placeholder},function(){return[e.display==="comma"?(p(),w(F,{key:0},[K(A(o.label||"empty"),1)],64)):e.display==="chip"?(p(),w(F,{key:1},[o.chipSelectedItems?(p(),w("span",Ur,A(o.label),1)):(p(),w(F,{key:1},[(p(!0),w(F,null,Ke(o.selectedNodes,function(s){return p(),w("div",m({key:s.key,class:e.cx("chipItem")},{ref_for:!0},e.ptm("chipItem")),[T(c,{class:M(e.cx("pcChip")),label:s.label,unstyled:e.unstyled,pt:e.ptm("pcChip")},null,8,["class","label","unstyled","pt"])],16)}),128)),o.emptyValue?(p(),w(F,{key:0},[K(A(e.placeholder||"empty"),1)],64)):S("",!0)],64))],64)):S("",!0)]})],16)],16),o.isClearIconVisible?O(e.$slots,"clearicon",{key:0,class:M(e.cx("clearIcon")),clearCallback:o.onClearClick},function(){return[(p(),C(R(e.clearIcon?"i":"TimesIcon"),m({ref:"clearIcon",class:[e.cx("clearIcon"),e.clearIcon],onClick:o.onClearClick},e.ptm("clearIcon"),{"data-pc-section":"clearicon"}),null,16,["class","onClick"]))]}):S("",!0),d("div",m({class:e.cx("dropdown"),role:"button","aria-haspopup":"tree","aria-expanded":a.overlayVisible},e.ptm("dropdown")),[O(e.$slots,e.$slots.dropdownicon?"dropdownicon":"triggericon",{class:M(e.cx("dropdownIcon"))},function(){return[(p(),C(R("ChevronDownIcon"),m({class:e.cx("dropdownIcon")},e.ptm("dropdownIcon")),null,16,["class"]))]})],16,Hr),T(h,{appendTo:e.appendTo},{default:I(function(){return[T(lt,m({name:"p-connected-overlay",onEnter:o.onOverlayEnter,onAfterEnter:o.onOverlayAfterEnter,onLeave:o.onOverlayLeave,onAfterLeave:o.onOverlayAfterLeave},e.ptm("transition")),{default:I(function(){return[a.overlayVisible?(p(),w("div",m({key:0,ref:o.overlayRef,onClick:t[8]||(t[8]=function(){return o.onOverlayClick&&o.onOverlayClick.apply(o,arguments)}),class:[e.cx("panel"),e.panelClass],onKeydown:t[9]||(t[9]=function(){return o.onOverlayKeydown&&o.onOverlayKeydown.apply(o,arguments)})},he(he({},e.panelProps),e.ptm("panel"))),[d("span",m({ref:"firstHiddenFocusableElementOnOverlay",role:"presentation",class:"p-hidden-accessible p-hidden-focusable",tabindex:0,onFocus:t[3]||(t[3]=function(){return o.onFirstHiddenFocus&&o.onFirstHiddenFocus.apply(o,arguments)})},e.ptm("hiddenFirstFocusableEl"),{"data-p-hidden-accessible":!0,"data-p-hidden-focusable":!0}),null,16),O(e.$slots,"header",{value:e.d_value,options:e.options}),d("div",m({class:e.cx("treeContainer"),style:{"max-height":e.scrollHeight}},e.ptm("treeContainer")),[T(g,{ref:"tree",id:o.listId,value:e.options,selectionMode:e.selectionMode,loading:e.loading,loadingIcon:e.loadingIcon,loadingMode:e.loadingMode,filter:e.filter,filterBy:e.filterBy,filterMode:e.filterMode,filterPlaceholder:e.filterPlaceholder,filterLocale:e.filterLocale,"onUpdate:selectionKeys":o.onSelectionChange,selectionKeys:e.d_value,expandedKeys:a.d_expandedKeys,"onUpdate:expandedKeys":o.onNodeToggle,metaKeySelection:e.metaKeySelection,onNodeExpand:t[4]||(t[4]=function(s){return e.$emit("node-expand",s)}),onNodeCollapse:t[5]||(t[5]=function(s){return e.$emit("node-collapse",s)}),onNodeSelect:o.onNodeSelect,onNodeUnselect:o.onNodeUnselect,onClick:t[6]||(t[6]=ot(function(){},["stop"])),level:0,unstyled:e.unstyled,pt:e.ptm("pcTree")},zt({_:2},[e.$slots.option?{name:"default",fn:I(function(s){return[O(e.$slots,"option",{node:s.node,expanded:s.expanded,selected:s.selected})]}),key:"0"}:void 0,e.$slots.itemtoggleicon?{name:"toggleicon",fn:I(function(s){return[O(e.$slots,"itemtoggleicon",{node:s.node,expanded:s.expanded,class:M(s.class)})]}),key:"1"}:e.$slots.itemtogglericon?{name:"togglericon",fn:I(function(s){return[O(e.$slots,"itemtogglericon",{node:s.node,expanded:s.expanded,class:M(s.class)})]}),key:"2"}:void 0,e.$slots.itemcheckboxicon?{name:"checkboxicon",fn:I(function(s){return[O(e.$slots,"itemcheckboxicon",{checked:s.checked,partialChecked:s.partialChecked,class:M(s.class)})]}),key:"3"}:void 0]),1032,["id","value","selectionMode","loading","loadingIcon","loadingMode","filter","filterBy","filterMode","filterPlaceholder","filterLocale","onUpdate:selectionKeys","selectionKeys","expandedKeys","onUpdate:expandedKeys","metaKeySelection","onNodeSelect","onNodeUnselect","unstyled","pt"]),o.emptyOptions&&!e.loading?(p(),w("div",m({key:0,class:e.cx("emptyMessage")},e.ptm("emptyMessage")),[O(e.$slots,"empty",{},function(){return[K(A(o.emptyMessageText),1)]})],16)):S("",!0)],16),O(e.$slots,"footer",{value:e.d_value,options:e.options}),d("span",m({ref:"lastHiddenFocusableElementOnOverlay",role:"presentation",class:"p-hidden-accessible p-hidden-focusable",tabindex:0,onFocus:t[7]||(t[7]=function(){return o.onLastHiddenFocus&&o.onLastHiddenFocus.apply(o,arguments)})},e.ptm("hiddenLastFocusableEl"),{"data-p-hidden-accessible":!0,"data-p-hidden-focusable":!0}),null,16)],16)):S("",!0)]}),_:3},16,["onEnter","onAfterEnter","onLeave","onAfterLeave"])]}),_:3},8,["appendTo"])],16)}zr.render=qr;var Wr=`
    .p-drawer {
        display: flex;
        flex-direction: column;
        transform: translate3d(0px, 0px, 0px);
        position: relative;
        transition: transform 0.3s;
        background: dt('drawer.background');
        color: dt('drawer.color');
        border: 1px solid dt('drawer.border.color');
        box-shadow: dt('drawer.shadow');
    }

    .p-drawer-content {
        overflow-y: auto;
        flex-grow: 1;
        padding: dt('drawer.content.padding');
    }

    .p-drawer-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        flex-shrink: 0;
        padding: dt('drawer.header.padding');
    }

    .p-drawer-footer {
        padding: dt('drawer.footer.padding');
    }

    .p-drawer-title {
        font-weight: dt('drawer.title.font.weight');
        font-size: dt('drawer.title.font.size');
    }

    .p-drawer-full .p-drawer {
        transition: none;
        transform: none;
        width: 100vw !important;
        height: 100vh !important;
        max-height: 100%;
        top: 0px !important;
        left: 0px !important;
        border-width: 1px;
    }

    .p-drawer-left .p-drawer-enter-from,
    .p-drawer-left .p-drawer-leave-to {
        transform: translateX(-100%);
    }

    .p-drawer-right .p-drawer-enter-from,
    .p-drawer-right .p-drawer-leave-to {
        transform: translateX(100%);
    }

    .p-drawer-top .p-drawer-enter-from,
    .p-drawer-top .p-drawer-leave-to {
        transform: translateY(-100%);
    }

    .p-drawer-bottom .p-drawer-enter-from,
    .p-drawer-bottom .p-drawer-leave-to {
        transform: translateY(100%);
    }

    .p-drawer-full .p-drawer-enter-from,
    .p-drawer-full .p-drawer-leave-to {
        opacity: 0;
    }

    .p-drawer-full .p-drawer-enter-active,
    .p-drawer-full .p-drawer-leave-active {
        transition: opacity 400ms cubic-bezier(0.25, 0.8, 0.25, 1);
    }

    .p-drawer-left .p-drawer {
        width: 20rem;
        height: 100%;
        border-inline-end-width: 1px;
    }

    .p-drawer-right .p-drawer {
        width: 20rem;
        height: 100%;
        border-inline-start-width: 1px;
    }

    .p-drawer-top .p-drawer {
        height: 10rem;
        width: 100%;
        border-block-end-width: 1px;
    }

    .p-drawer-bottom .p-drawer {
        height: 10rem;
        width: 100%;
        border-block-start-width: 1px;
    }

    .p-drawer-left .p-drawer-content,
    .p-drawer-right .p-drawer-content,
    .p-drawer-top .p-drawer-content,
    .p-drawer-bottom .p-drawer-content {
        width: 100%;
        height: 100%;
    }

    .p-drawer-open {
        display: flex;
    }

    .p-drawer-mask:dir(rtl) {
        flex-direction: row-reverse;
    }
`,Zr={mask:function(t){var n=t.position,r=t.modal;return{position:"fixed",height:"100%",width:"100%",left:0,top:0,display:"flex",justifyContent:n==="left"?"flex-start":n==="right"?"flex-end":"center",alignItems:n==="top"?"flex-start":n==="bottom"?"flex-end":"center",pointerEvents:r?"auto":"none"}},root:{pointerEvents:"auto"}},Yr={mask:function(t){var n=t.instance,r=t.props,a=["left","right","top","bottom"],o=a.find(function(c){return c===r.position});return["p-drawer-mask",{"p-overlay-mask p-overlay-mask-enter":r.modal,"p-drawer-open":n.containerVisible,"p-drawer-full":n.fullScreen},o?"p-drawer-".concat(o):""]},root:function(t){var n=t.instance;return["p-drawer p-component",{"p-drawer-full":n.fullScreen}]},header:"p-drawer-header",title:"p-drawer-title",pcCloseButton:"p-drawer-close-button",content:"p-drawer-content",footer:"p-drawer-footer"},Gr=me.extend({name:"drawer",style:Wr,classes:Yr,inlineStyles:Zr}),Xr={name:"BaseDrawer",extends:Ae,props:{visible:{type:Boolean,default:!1},position:{type:String,default:"left"},header:{type:null,default:null},baseZIndex:{type:Number,default:0},autoZIndex:{type:Boolean,default:!0},dismissable:{type:Boolean,default:!0},showCloseIcon:{type:Boolean,default:!0},closeButtonProps:{type:Object,default:function(){return{severity:"secondary",text:!0,rounded:!0}}},closeIcon:{type:String,default:void 0},modal:{type:Boolean,default:!0},blockScroll:{type:Boolean,default:!1}},style:Gr,provide:function(){return{$pcDrawer:this,$parentInstance:this}}};function se(e){"@babel/helpers - typeof";return se=typeof Symbol=="function"&&typeof Symbol.iterator=="symbol"?function(t){return typeof t}:function(t){return t&&typeof Symbol=="function"&&t.constructor===Symbol&&t!==Symbol.prototype?"symbol":typeof t},se(e)}function Ce(e,t,n){return(t=Jr(t))in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function Jr(e){var t=Qr(e,"string");return se(t)=="symbol"?t:t+""}function Qr(e,t){if(se(e)!="object"||!e)return e;var n=e[Symbol.toPrimitive];if(n!==void 0){var r=n.call(e,t);if(se(r)!="object")return r;throw new TypeError("@@toPrimitive must return a primitive value.")}return(t==="string"?String:Number)(e)}var _r={name:"Drawer",extends:Xr,inheritAttrs:!1,emits:["update:visible","show","after-show","hide","after-hide","before-hide"],data:function(){return{containerVisible:this.visible}},container:null,mask:null,content:null,headerContainer:null,footerContainer:null,closeButton:null,outsideClickListener:null,documentKeydownListener:null,watch:{dismissable:function(t){t?this.enableDocumentSettings():this.disableDocumentSettings()}},updated:function(){this.visible&&(this.containerVisible=this.visible)},beforeUnmount:function(){this.disableDocumentSettings(),this.mask&&this.autoZIndex&&Q.clear(this.mask),this.container=null,this.mask=null},methods:{hide:function(){this.$emit("update:visible",!1)},onEnter:function(){this.$emit("show"),this.focus(),this.bindDocumentKeyDownListener(),this.autoZIndex&&Q.set("modal",this.mask,this.baseZIndex||this.$primevue.config.zIndex.modal)},onAfterEnter:function(){this.enableDocumentSettings(),this.$emit("after-show")},onBeforeLeave:function(){this.modal&&!this.isUnstyled&&Rt(this.mask,"p-overlay-mask-leave"),this.$emit("before-hide")},onLeave:function(){this.$emit("hide")},onAfterLeave:function(){this.autoZIndex&&Q.clear(this.mask),this.unbindDocumentKeyDownListener(),this.containerVisible=!1,this.disableDocumentSettings(),this.$emit("after-hide")},onMaskClick:function(t){this.dismissable&&this.modal&&this.mask===t.target&&this.hide()},focus:function(){var t=function(a){return a&&a.querySelector("[autofocus]")},n=this.$slots.header&&t(this.headerContainer);n||(n=this.$slots.default&&t(this.container),n||(n=this.$slots.footer&&t(this.footerContainer),n||(n=this.closeButton))),n&&X(n)},enableDocumentSettings:function(){this.dismissable&&!this.modal&&this.bindOutsideClickListener(),this.blockScroll&&Bt()},disableDocumentSettings:function(){this.unbindOutsideClickListener(),this.blockScroll&&Vt()},onKeydown:function(t){t.code==="Escape"&&this.hide()},containerRef:function(t){this.container=t},maskRef:function(t){this.mask=t},contentRef:function(t){this.content=t},headerContainerRef:function(t){this.headerContainer=t},footerContainerRef:function(t){this.footerContainer=t},closeButtonRef:function(t){this.closeButton=t?t.$el:void 0},bindDocumentKeyDownListener:function(){this.documentKeydownListener||(this.documentKeydownListener=this.onKeydown,document.addEventListener("keydown",this.documentKeydownListener))},unbindDocumentKeyDownListener:function(){this.documentKeydownListener&&(document.removeEventListener("keydown",this.documentKeydownListener),this.documentKeydownListener=null)},bindOutsideClickListener:function(){var t=this;this.outsideClickListener||(this.outsideClickListener=function(n){t.isOutsideClicked(n)&&t.hide()},document.addEventListener("click",this.outsideClickListener,!0))},unbindOutsideClickListener:function(){this.outsideClickListener&&(document.removeEventListener("click",this.outsideClickListener,!0),this.outsideClickListener=null)},isOutsideClicked:function(t){return this.container&&!this.container.contains(t.target)}},computed:{fullScreen:function(){return this.position==="full"},closeAriaLabel:function(){return this.$primevue.config.locale.aria?this.$primevue.config.locale.aria.close:void 0},dataP:function(){return ye(Ce(Ce(Ce({"full-screen":this.position==="full"},this.position,this.position),"open",this.containerVisible),"modal",this.modal))}},directives:{focustrap:Zt},components:{Button:_t,Portal:at,TimesIcon:dt}},eo=["data-p"],to=["aria-modal","data-p"];function no(e,t,n,r,a,o){var c=D("Button"),g=D("Portal"),h=nt("focustrap");return p(),C(g,null,{default:I(function(){return[a.containerVisible?(p(),w("div",m({key:0,ref:o.maskRef,onMousedown:t[0]||(t[0]=function(){return o.onMaskClick&&o.onMaskClick.apply(o,arguments)}),class:e.cx("mask"),style:e.sx("mask",!0,{position:e.position,modal:e.modal}),"data-p":o.dataP},e.ptm("mask")),[T(lt,m({name:"p-drawer",onEnter:o.onEnter,onAfterEnter:o.onAfterEnter,onBeforeLeave:o.onBeforeLeave,onLeave:o.onLeave,onAfterLeave:o.onAfterLeave,appear:""},e.ptm("transition")),{default:I(function(){return[e.visible?rt((p(),w("div",m({key:0,ref:o.containerRef,class:e.cx("root"),style:e.sx("root"),role:"complementary","aria-modal":e.modal,"data-p":o.dataP},e.ptmi("root")),[e.$slots.container?O(e.$slots,"container",{key:0,closeCallback:o.hide}):(p(),w(F,{key:1},[d("div",m({ref:o.headerContainerRef,class:e.cx("header")},e.ptm("header")),[O(e.$slots,"header",{class:M(e.cx("title"))},function(){return[e.header?(p(),w("div",m({key:0,class:e.cx("title")},e.ptm("title")),A(e.header),17)):S("",!0)]}),e.showCloseIcon?O(e.$slots,"closebutton",{key:0,closeCallback:o.hide},function(){return[T(c,m({ref:o.closeButtonRef,type:"button",class:e.cx("pcCloseButton"),"aria-label":o.closeAriaLabel,unstyled:e.unstyled,onClick:o.hide},e.closeButtonProps,{pt:e.ptm("pcCloseButton"),"data-pc-group-section":"iconcontainer"}),{icon:I(function(s){return[O(e.$slots,"closeicon",{},function(){return[(p(),C(R(e.closeIcon?"span":"TimesIcon"),m({class:[e.closeIcon,s.class]},e.ptm("pcCloseButton").icon),null,16,["class"]))]})]}),_:3},16,["class","aria-label","unstyled","onClick","pt"])]}):S("",!0)],16),d("div",m({ref:o.contentRef,class:e.cx("content")},e.ptm("content")),[O(e.$slots,"default")],16),e.$slots.footer?(p(),w("div",m({key:0,ref:o.footerContainerRef,class:e.cx("footer")},e.ptm("footer")),[O(e.$slots,"footer")],16)):S("",!0)],64))],16,to)),[[h]]):S("",!0)]}),_:3},16,["onEnter","onAfterEnter","onBeforeLeave","onLeave","onAfterLeave"])],16,eo)):S("",!0)]}),_:3})}_r.render=no;var go={name:"Dropdown",extends:J,mounted:function(){console.warn("Deprecated since v4. Use Select component instead.")}};export{mo as _,zr as a,go as b,_r as c,ft as s,we as t,rn as u};
