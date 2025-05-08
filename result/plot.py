import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

# Загрузка данных
data = pd.read_csv('results.csv')

# Настройка стиля графиков
sns.set(style='whitegrid')

# 1. График вероятности простоя
plt.figure(figsize=(10, 6))
plt.plot(data['Lambda'], data['P0_Theory'], 'b-', label='Теоретическая')
plt.plot(data['Lambda'], data['P0_Experiment'], 'ro', label='Экспериментальная')
plt.title('Вероятность простоя системы')
plt.xlabel('Интенсивность входного потока (λ)')
plt.ylabel('P0')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.savefig('idle_probability.png')
plt.close()

# 2. График вероятности отказа
plt.figure(figsize=(10, 6))
plt.plot(data['Lambda'], data['PReject_Theory'], 'b-', label='Теоретическая')
plt.plot(data['Lambda'], data['PReject_Experiment'], 'ro', label='Экспериментальная')
plt.title('Вероятность отказа системы')
plt.xlabel('Интенсивность входного потока (λ)')
plt.ylabel('P отказа')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.savefig('reject_probability.png')
plt.close()

# 3. График относительной пропускной способности
plt.figure(figsize=(10, 6))
plt.plot(data['Lambda'], data['Q_Theory'], 'b-', label='Теоретическая')
plt.plot(data['Lambda'], data['Q_Experiment'], 'ro', label='Экспериментальная')
plt.title('Относительная пропускная способность')
plt.xlabel('Интенсивность входного потока (λ)')
plt.ylabel('Q')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.savefig('relative_throughput.png')
plt.close()

# 4. График абсолютной пропускной способности
plt.figure(figsize=(10, 6))
plt.plot(data['Lambda'], data['A_Theory'], 'b-', label='Теоретическая')
plt.plot(data['Lambda'], data['A_Experiment'], 'ro', label='Экспериментальная')
plt.title('Абсолютная пропускная способность')
plt.xlabel('Интенсивность входного потока (λ)')
plt.ylabel('A (запросов/сек)')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.savefig('absolute_throughput.png')
plt.close()

# 5. График среднего числа занятых каналов
plt.figure(figsize=(10, 6))
plt.plot(data['Lambda'], data['K_Theory'], 'b-', label='Теоретическая')
plt.plot(data['Lambda'], data['K_Experiment'], 'ro', label='Экспериментальная')
plt.title('Среднее число занятых каналов')
plt.xlabel('Интенсивность входного потока (λ)')
plt.ylabel('k')
plt.legend()
plt.grid(True)
plt.tight_layout()
plt.savefig('busy_channels.png')
plt.close()

print("Графики успешно сохранены в отдельных файлах:")
print("- idle_probability.png")
print("- reject_probability.png")
print("- relative_throughput.png")
print("- absolute_throughput.png")
print("- busy_channels.png")