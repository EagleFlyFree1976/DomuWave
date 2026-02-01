

<template>

  <MenuTree />

  <div id="content-wrapper" class="d-flex flex-column">

    <!-- Main Content -->
    <div id="content">
      <nav class="navbar navbar-expand navbar-light bg-white topbar mb-4 static-top shadow">

        <!-- Sidebar Toggle (Topbar) -->
        <button id="sidebarToggleTop" class="btn btn-link d-md-none rounded-circle mr-3">
          <i class="fa fa-bars"></i>
        </button>

        <!-- Topbar Search -->
        <form class="d-none d-sm-inline-block form-inline mr-auto ml-md-3 my-2 my-md-0 mw-100 navbar-search">
          <div class="input-group">
            <input type="text" class="form-control bg-light border-0 small" placeholder="Search for..." aria-label="Search" aria-describedby="basic-addon2">
            <div class="input-group-append">
              <button class="btn btn-primary" type="button">
                <i class="fas fa-search fa-sm"></i>
              </button>
            </div>
          </div>
        </form>

        <!-- Topbar Navbar -->
        <ul class="navbar-nav ml-auto">

          <!-- Nav Item - Search Dropdown (Visible Only XS) -->
          <li class="nav-item dropdown no-arrow d-sm-none">
            <a class="nav-link dropdown-toggle" href="#" id="searchDropdown" role="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
              <i class="fas fa-search fa-fw"></i>
            </a>
            <!-- Dropdown - Messages -->
            <div class="dropdown-menu dropdown-menu-right p-3 shadow animated--grow-in" aria-labelledby="searchDropdown">
              <form class="form-inline mr-auto w-100 navbar-search">
                <div class="input-group">
                  <input type="text" class="form-control bg-light border-0 small" placeholder="Search for..." aria-label="Search" aria-describedby="basic-addon2">
                  <div class="input-group-append">
                    <button class="btn btn-primary" type="button">
                      <i class="fas fa-search fa-sm"></i>
                    </button>
                  </div>
                </div>
              </form>
            </div>
          </li>

       

          <div class="topbar-divider d-none d-sm-block"></div>

          <!-- Nav Item - User Information -->
          <li class="nav-item dropdown no-arrow">
            <a class="nav-link dropdown-toggle1" href="#" id="userDropdown" role="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
              <span class="mr-2 d-none d-lg-inline text-gray-600 small">
                <a class="w-50" href="#" @click="toggle" v-if="auth.user != null">
                  <img style="width:50px" class="img-profile rounded-circle" src="@/assets/undraw_profile.svg">
                </a>
                <Menu ref="menu" id="overlay_menu" :model="items" :popup="true">
                  <template #item="{ item, props }">
                    <router-link v-if="item.route" v-slot="{ href, navigate }" :to="item.route" custom>
                      <a v-ripple :href="href" v-bind="props.action" @click="navigate">
                        <span :class="item.icon" />
                        <span class="ml-2">{{ item.label }}</span>
                      </a>
                    </router-link>

                    <a v-else v-ripple class="flex items-center" v-bind="props.action">
                      <span :class="item.icon" />
                      <span>{{ item.label }}</span>
                      <Badge v-if="item.badge" class="ml-auto" :value="item.badge" />
                      <span v-if="item.shortcut" class="ml-auto border border-surface rounded bg-emphasis text-muted-color text-xs p-1">{{ item.shortcut }}</span>
                    </a>
                  </template>
                  <template #end>

                  </template>

                </Menu>
              </span>
              <!--<img class="img-profile rounded-circle" src="img/undraw_profile.svg">-->
            </a>
            <!-- Dropdown - User Information -->
           
          </li>

        </ul>

      </nav>
       
      <!-- End of Topbar -->
      <!-- Begin Page Content -->
      <div class="container-fluid">

        <BlockUI :blocked="DomuWaveStore.loading" fullScreen>  </BlockUI>
        <Messages></Messages>
        <RouterView />

      </div>
    </div>
    <!-- Footer -->
    <footer class="sticky-footer bg-white">
      <div class="container my-auto">
        <div class="copyright text-center my-auto">
          <span>Copyright &copy; Your Website 2021</span>
        </div>
      </div>
    </footer>

    <!-- End of Footer -->
  </div>


</template>

<script setup>
  import { RouterLink, RouterView, useRouter } from 'vue-router'
  import MenuTree from './components/Menu/MenuTree.vue'
  import UserInfo from './components/UserInfo.vue'
  import Toolbar from 'primevue/toolbar';
  import Menu from 'primevue/menu';
  import Button from 'primevue/button';
  import { useDomuWaveStore } from './stores/DomuWaveStore'
  import Messages from './components/Messages.vue'
  import { ref, onMounted, computed } from 'vue'
  import BlockUI from 'primevue/blockui';
  import { useAuthStore } from '@/stores/authStore'
  import { eventBus } from '@/code/eventBus'
  import { useMessageStore } from '@/stores/messageStore'
    import { MESSAGES, TYPES } from '@/code/messages'
  const auth = useAuthStore();
  const DomuWaveStore = useDomuWaveStore();
  const messageStore = useMessageStore();
  const router = useRouter();
  const menu = ref();
  //DomuWaveStore.setUser(  { Name: "Orazio", Surname: "Greco", FullName: "Orazio Greco" });
  
  const items = computed(() => {
    return [


      {
        label: auth != null && auth.user != null ? auth.user.fullName : "",
        route: '/profile',
        icon: 'fas fa-user',
        shortcut: '⌘+O',


      },
      {
        label: 'Messages',
        route: '/messages',
        icon: 'fas fa-envelope',
        badge: 2
      },
      {
        label: 'Logout',
        route: '/logout',
        icon: 'fas fa-sign-out-alt',

      }

    ];
  });

  const toggle = (event) => {
    menu.value.toggle(event);
  };

  onMounted(() => {
    eventBus.on('unauthorized', () => {
      DomuWaveStore.stopLoading();
      messageStore.setMessages('Sessione scaduta. Effettua nuovamente il login.', TYPES.error);
   
      router.push('/login');
    })
  })
</script>
