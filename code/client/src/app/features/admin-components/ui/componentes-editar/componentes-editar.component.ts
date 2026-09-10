import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  Output,
  signal,
  WritableSignal,
} from '@angular/core';
import { ValidatedFormsModule } from '@shared/lib/forms';
import { Componente, Equipos } from '@entities/admin';
import { ComponenteService } from '@entities/component';
import { EquipoService } from '@entities/equipment';
import { BaseTablaComponent } from '@shared/lib/admin-table';
import { extractErrorMessage } from '@shared/lib/error';
import {
  Aviso,
  CustomSelectComponent,
  MostrarerrorComponent,
  OpcionSelect,
  ToastService,
} from '@shared/ui';
@Component({
  selector: 'app-componentes-editar',
  standalone: true,
  imports: [
    ValidatedFormsModule,
    MostrarerrorComponent,
    Aviso,
    CustomSelectComponent,
  ],
  templateUrl: './componentes-editar.component.html',
  styleUrl: './componentes-editar.component.css',
})
export class ComponentesEditarComponent extends BaseTablaComponent {
  @Input() botoneditar: WritableSignal<boolean> = signal(true);
  @Output() actualizar: EventEmitter<void> = new EventEmitter<void>();
  @Input() componente: Componente = new Componente();
  equipos: Equipos[] = [];
  get equiposOpciones(): OpcionSelect[] {
    return this.equipos.map((equipo) => ({
      value: equipo.Id,
      label: `${equipo.NombreGrupoEquipo} ${equipo.CodigoImt}`,
    }));
  }

  constructor(
    private readonly componenteService: ComponenteService,
    private equiposAPI: EquipoService,
    private readonly toast: ToastService,
  ) {
    super();
  }
  ngOnInit() {
    this.cargarEquipos();
  }
  cargarEquipos() {
    this.equiposAPI.obtenerEquipos().subscribe({
      next: (data: Equipos[]) => {
        this.equipos = data;
      },
      error: (error) => {
        const errorMsg = extractErrorMessage(
          error,
          'Error al obtener los equipos , intente mas tarde',
        );
        this.mensajeerror = errorMsg;
        this.error.set(true);
      },
    });
  }
  validaredicion() {
    this.mensajeaviso = 'Estas seguro de editar este componente?';
    this.aviso.set(true);
  }
  confirmar() {
    if (!this.iniciarEnvio()) return;
    this.componenteService.actualizarComponente(this.componente).subscribe({
      next: (_response) => {
        this.cerrar();
        this.finalizarEnvio();
        this.toast.success('Componente actualizado satisfactoriamente.');
        this.actualizar.emit();
      },
      error: (error) => {
        const errorMsg = extractErrorMessage(
          error,
          'Error al actualizar el componenete , intente mas tarde',
        );
        this.mensajeerror = errorMsg;
        this.error.set(true);
        this.finalizarEnvio();
      },
    });
  }
  cerrar() {
    this.botoneditar.set(false);
  }
  @HostListener('click', ['$event'])
  onOverlayClick(event: MouseEvent) {
    if (event.target === event.currentTarget) this.cerrar();
  }
}
