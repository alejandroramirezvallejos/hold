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
import { Categorias } from '@entities/admin';
import { CategoriaService } from '@entities/category';
import { BaseTablaComponent } from '@shared/lib/admin-table';
import { extractErrorMessage } from '@shared/lib/error';
import { Aviso, MostrarerrorComponent, ToastService } from '@shared/ui';
@Component({
  selector: 'app-categorias-editar',
  standalone: true,
  imports: [ValidatedFormsModule, MostrarerrorComponent, Aviso],
  templateUrl: './categorias-editar.component.html',
  styleUrl: './categorias-editar.component.css',
})
export class CategoriasEditarComponent extends BaseTablaComponent {
  @Input() botoneditar: WritableSignal<boolean> = signal(true);
  @Output() actualizar: EventEmitter<void> = new EventEmitter<void>();
  @Input() categoria: Categorias = new Categorias();

  constructor(
    private readonly categoriaService: CategoriaService,
    private readonly toast: ToastService,
  ) {
    super();
  }

  validaredicion() {
    if (!this.categoria.Nombre || this.categoria.Nombre.trim() === '') {
      this.mensajeerror = 'Por favor ingrese el nombre de la categoría';
      this.error.set(true);
      return;
    }
    this.mensajeaviso = '¿Está seguro de que desea actualizar la categoría?';
    this.aviso.set(true);
  }

  confirmar() {
    if (!this.iniciarEnvio()) return;
    this.categoriaService.actualizarCategoria(this.categoria).subscribe({
      next: (_response) => {
        this.cerrar();
        this.finalizarEnvio();
        this.toast.success('Categoría actualizada con éxito.');
        this.actualizar.emit();
      },
      error: (error) => {
        const errorMsg = extractErrorMessage(
          error,
          'No se pudo actualizar la Categoria intente mas tarde',
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
