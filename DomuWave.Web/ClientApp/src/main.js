import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import Tooltip from 'primevue/tooltip'

// ── Componenti PrimeVue ──────────────────────────────────────────────────────
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import MultiSelect from 'primevue/multiselect'
import DatePicker from 'primevue/datepicker'
import Checkbox from 'primevue/checkbox'
import RadioButton from 'primevue/radiobutton'
import ToggleSwitch from 'primevue/toggleswitch'
import Password from 'primevue/password'

import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ColumnGroup from 'primevue/columngroup'
import Row from 'primevue/row'

import Card from 'primevue/card'
import Panel from 'primevue/panel'
import Fieldset from 'primevue/fieldset'
import Divider from 'primevue/divider'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Accordion from 'primevue/accordion'
import AccordionPanel from 'primevue/accordionpanel'
import AccordionHeader from 'primevue/accordionheader'
import AccordionContent from 'primevue/accordioncontent'

import Dialog from 'primevue/dialog'
import ConfirmDialog from 'primevue/confirmdialog'
import Drawer from 'primevue/drawer'
import Popover from 'primevue/popover'

import Toast from 'primevue/toast'
import Message from 'primevue/message'
import Tag from 'primevue/tag'
import Badge from 'primevue/badge'
import Avatar from 'primevue/avatar'
import Chip from 'primevue/chip'
import ProgressBar from 'primevue/progressbar'
import ProgressSpinner from 'primevue/progressspinner'
import Skeleton from 'primevue/skeleton'

import Menu from 'primevue/menu'
import Menubar from 'primevue/menubar'
import Breadcrumb from 'primevue/breadcrumb'
import Steps from 'primevue/steps'
import Toolbar from 'primevue/toolbar'

import FileUpload from 'primevue/fileupload'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import FloatLabel from 'primevue/floatlabel'
import InputGroup from 'primevue/inputgroup'
import InputGroupAddon from 'primevue/inputgroupaddon'

import router from '@/router'
import App from './App.vue'

import 'primeicons/primeicons.css'
import '@/assets/main.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: '.p-dark',
      cssLayer: false,
    },
  },
})
app.use(ToastService)
app.use(ConfirmationService)
app.directive('tooltip', Tooltip)

// ── Registrazione globale componenti ────────────────────────────────────────
app.component('Button', Button)
app.component('InputText', InputText)
app.component('InputNumber', InputNumber)
app.component('Textarea', Textarea)
app.component('Select', Select)
app.component('MultiSelect', MultiSelect)
app.component('DatePicker', DatePicker)
app.component('Checkbox', Checkbox)
app.component('RadioButton', RadioButton)
app.component('ToggleSwitch', ToggleSwitch)
app.component('Password', Password)

app.component('DataTable', DataTable)
app.component('Column', Column)
app.component('ColumnGroup', ColumnGroup)
app.component('Row', Row)

app.component('Card', Card)
app.component('Panel', Panel)
app.component('Fieldset', Fieldset)
app.component('Divider', Divider)
app.component('Tabs', Tabs)
app.component('TabList', TabList)
app.component('Tab', Tab)
app.component('TabPanels', TabPanels)
app.component('TabPanel', TabPanel)
app.component('Accordion', Accordion)
app.component('AccordionPanel', AccordionPanel)
app.component('AccordionHeader', AccordionHeader)
app.component('AccordionContent', AccordionContent)

app.component('Dialog', Dialog)
app.component('ConfirmDialog', ConfirmDialog)
app.component('Drawer', Drawer)
app.component('Popover', Popover)

app.component('Toast', Toast)
app.component('Message', Message)
app.component('Tag', Tag)
app.component('Badge', Badge)
app.component('Avatar', Avatar)
app.component('Chip', Chip)
app.component('ProgressBar', ProgressBar)
app.component('ProgressSpinner', ProgressSpinner)
app.component('Skeleton', Skeleton)

app.component('Menu', Menu)
app.component('Menubar', Menubar)
app.component('Breadcrumb', Breadcrumb)
app.component('Steps', Steps)
app.component('Toolbar', Toolbar)

app.component('FileUpload', FileUpload)
app.component('IconField', IconField)
app.component('InputIcon', InputIcon)
app.component('FloatLabel', FloatLabel)
app.component('InputGroup', InputGroup)
app.component('InputGroupAddon', InputGroupAddon)

app.mount('#app')

// Applica dark theme di default
document.documentElement.classList.add('p-dark')
